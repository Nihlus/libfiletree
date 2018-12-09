//
//  TreeSerializer.cs
//
//  Copyright (c) 2018 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileTree.Tree.Nodes;
using FileTree.Tree.Serialized;
using JetBrains.Annotations;
using liblistfile;
using Warcraft.Core.Extensions;

namespace FileTree.Tree
{
    /// <summary>
    /// Serializes node trees.
    /// </summary>
    [PublicAPI]
    public class TreeSerializer : IDisposable
    {
        [NotNull]
        private readonly Stream _outputStream;
        private readonly bool _keepStreamOpen;

        /// <summary>
        /// Holds byte offsets to node names in the name block, relative to the start of the name block.
        /// </summary>
        [NotNull]
        private readonly Dictionary<string, long> _relativeNameOffsets;

        /// <summary>
        /// Holds absolute byte offsets to nodes in the output.
        /// </summary>
        [NotNull]
        private readonly Dictionary<Node, long> _absoluteNodeOffsets;

        private long _nodeBlockOffset;
        private long _nameBlockOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeSerializer"/> class.
        /// </summary>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="keepStreamOpen">Whether to keep the output stream open after finishing.</param>
        [PublicAPI]
        public TreeSerializer([NotNull] Stream outputStream, bool keepStreamOpen = false)
        {
            _outputStream = outputStream;
            _keepStreamOpen = keepStreamOpen;

            _relativeNameOffsets = new Dictionary<string, long>();
            _absoluteNodeOffsets = new Dictionary<Node, long>();
        }

        /// <summary>
        /// Serializes the given tree to the output stream.
        /// </summary>
        /// <param name="root">The root node of the tree to serialize.</param>
        /// <param name="ct">The cancellation token to use.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [PublicAPI, NotNull]
        public Task SerializeAsync([NotNull] Node root, CancellationToken ct = default)
        {
            var flattenedTree = FlattenTree(root).ToList();

            // 1: Build name block
            var nameBlock = CreateNameBlock(flattenedTree);

            // 2: Build layout
            // 2.1: Order is header, root, nodes, names, sorting lists
            long currentLayoutOffset = 0;
            currentLayoutOffset += SerializedTree.HeaderSize;

            // Save the node block offset
            _nodeBlockOffset = currentLayoutOffset;

            foreach (var node in flattenedTree)
            {
                ct.ThrowIfCancellationRequested();

                _absoluteNodeOffsets.Add(node, currentLayoutOffset);
                currentLayoutOffset += GetSerializedSize(node);
            }

            _nameBlockOffset = currentLayoutOffset;

            // Now, we can begin writing
            using (var writer = new BinaryWriter(_outputStream, Encoding.Default, _keepStreamOpen))
            {
                writer.Write(OptimizedNodeTree.Version);
                writer.Write(_nodeBlockOffset);
                writer.Write(_nameBlockOffset);

                foreach (var node in flattenedTree)
                {
                    ct.ThrowIfCancellationRequested();

                    SerializeNode(node, writer);
                }

                writer.Write(nameBlock);
            }

            return _outputStream.FlushAsync(ct);
        }

        /// <summary>
        /// Flattens the given tree into a plain list of nodes.
        /// </summary>
        /// <param name="root">The root of the tree.</param>
        /// <returns>A flattened list of nodes.</returns>
        [NotNull, ItemNotNull]
        private IEnumerable<Node> FlattenTree([NotNull] Node root)
        {
            var stack = new Stack<Node>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;

                foreach (var child in current.Children)
                {
                    stack.Push(child);
                }
            }
        }

        /// <summary>
        /// Creates a name block for the given tree.
        /// </summary>
        /// <param name="nodes">The nodes in the tree.</param>
        /// <returns>The serialized name block.</returns>
        [NotNull]
        private byte[] CreateNameBlock([NotNull, ItemNotNull] IEnumerable<Node> nodes)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms, Encoding.Default))
                {
                    foreach (var name in nodes.Select(n => n.Name).Distinct())
                    {
                        _relativeNameOffsets.Add(name, bw.BaseStream.Position);

                        bw.WriteNullTerminatedString(name);
                    }
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Serializes the given node.
        /// </summary>
        /// <param name="node">The node to serialize.</param>
        /// <param name="writer">The binary writer to use for serialization.</param>
        private void SerializeNode([NotNull] Node node, [NotNull] BinaryWriter writer)
        {
            writer.Write((uint)node.Type);
            writer.Write((ulong)node.FileType);

            var nameOffset = _nameBlockOffset + _relativeNameOffsets[node.Name];
            writer.Write(nameOffset);

            long parentOffset;
            if (node.Parent is null)
            {
                parentOffset = -1;
            }
            else
            {
                parentOffset = _absoluteNodeOffsets[node.Parent];
            }

            writer.Write(parentOffset);

            writer.Write((ulong)node.Children.Count);
            foreach (var child in node.Children)
            {
                writer.Write(_absoluteNodeOffsets[child]);
            }

            if (node is VirtualNode virtualNode)
            {
                writer.Write((ulong)virtualNode.HardNodes.Count);
                foreach (var hardNode in virtualNode.HardNodes)
                {
                    writer.Write(_absoluteNodeOffsets[hardNode]);
                }
            }
        }

        /// <summary>
        /// Gets the serialized size of a node in bytes.
        /// </summary>
        /// <param name="node">The node to calculate the size of.</param>
        /// <returns>The byte size of the node.</returns>
        private long GetSerializedSize([NotNull] Node node)
        {
            long size = 0;

            // Type
            size += sizeof(uint);

            // FileType
            size += sizeof(ulong);

            // NameOffset
            size += sizeof(ulong);

            // ParentOffset
            size += sizeof(ulong);

            // ChildCount
            size += sizeof(ulong);

            // ChildOffsets
            size += node.Children.Count * sizeof(ulong);

            if (node is VirtualNode virtualNode)
            {
                // HardNodeCount
                size += sizeof(ulong);

                // HardNodeOffsets
                size += virtualNode.HardNodes.Count * sizeof(ulong);
            }

            return size;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_keepStreamOpen)
            {
                _outputStream.Dispose();
            }
        }
    }
}
