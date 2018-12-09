//
//  SerializedTree.cs
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
using JetBrains.Annotations;
using Warcraft.Core.Extensions;

namespace FileTree.Tree.Serialized
{
    /// <summary>
    /// An optimized tree of object nodes.
    /// Each <see cref="SerializedTree"/> (when serialized) is structured as follows:
    ///
    /// uint Version
    /// ulong nodesOffset
    /// ulong namesOffset
    /// Node RootNode
    /// Node[] Nodes
    /// char[] Names
    ///
    /// The reader only supports one given version of the file format at a time.
    /// </summary>
    [PublicAPI]
    public class SerializedTree : IDisposable
    {
        /// <summary>
        /// The current version of the node tree format.
        /// </summary>
        [PublicAPI]
        public const uint Version = 3;

        /// <summary>
        /// The size of the header of the tree.
        /// </summary>
        [PublicAPI]
        public const long HeaderSize = sizeof(uint) + (sizeof(long) * 2);

        private readonly long _nodesOffset;
        private readonly long _namesOffset;

        private readonly object _readerLock = new object();
        private readonly BinaryReader _treeReader;

        private readonly Dictionary<ulong, SerializedNode> _cachedNodes = new Dictionary<ulong, SerializedNode>();

        private readonly Dictionary<SerializedNode, ulong> _cachedOffsets = new Dictionary<SerializedNode, ulong>();
        private readonly Dictionary<SerializedNode, string> _cachedNames = new Dictionary<SerializedNode, string>();

        /// <summary>
        /// Gets the absolute root node of the tree.
        /// </summary>
        [PublicAPI, NotNull]
        public SerializedNode Root
        {
            get
            {
                if (_internalRoot != null)
                {
                    return _internalRoot;
                }

                _internalRoot = GetNode((ulong)_nodesOffset);
                return _internalRoot;
            }
        }

        [CanBeNull]
        private SerializedNode _internalRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedTree"/> class.
        /// </summary>
        /// <param name="treeStream">The stream to read the tree from.</param>
        [PublicAPI]
        public SerializedTree([NotNull] Stream treeStream)
        {
            _treeReader = new BinaryReader(treeStream);

            var storedVersion = _treeReader.ReadUInt32();
            if (storedVersion != Version)
            {
                // Do whatever functionality switching is needed
                throw new NotSupportedException();
            }

            // Latest implementation
            _nodesOffset = _treeReader.ReadInt64();
            _namesOffset = _treeReader.ReadInt64();
        }

        /// <summary>
        /// Gets a node from the specified offset in the tree.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The node.</returns>
        [PublicAPI, NotNull]
        public SerializedNode GetNode(ulong offset)
        {
            if (offset < (ulong)_nodesOffset)
            {
                throw new InvalidOperationException("The offset did not fall inside the node block.");
            }

            if (_cachedNodes.ContainsKey(offset))
            {
                return _cachedNodes[offset];
            }

            // Nodes may be read from multiple threads at any time due to async/await patterns, so we
            // lock the reader
            lock (_readerLock)
            {
                var newNode = SerializedNode.ReadNode(_treeReader, offset);

                _cachedNodes.Add(offset, newNode);
                _cachedOffsets.Add(newNode, offset);
                return newNode;
            }
        }

        /// <summary>
        /// Gets the absolute offset of a given node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The offset.</returns>
        [PublicAPI]
        public ulong GetNodeOffset([NotNull] SerializedNode node)
        {
            if (_cachedOffsets.ContainsKey(node))
            {
                return _cachedOffsets[node];
            }

            throw new InvalidOperationException("This node has not been read from this tree.");
        }

        /// <summary>
        /// Gets the name of a given node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The name.</returns>
        [PublicAPI, NotNull]
        public string GetNodeName([NotNull] SerializedNode node)
        {
            if (node.NameOffset < 0)
            {
                return string.Empty;
            }

            if (_cachedNames.ContainsKey(node))
            {
                return _cachedNames[node];
            }

            lock (_readerLock)
            {
                _treeReader.BaseStream.Position = node.NameOffset;
                var name = _treeReader.ReadNullTerminatedString();

                _cachedNames.Add(node, name);

                return name;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _treeReader?.Dispose();
        }
    }
}
