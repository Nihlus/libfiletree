//
//  SerializedNode.cs
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

using System.Collections.Generic;
using System.IO;
using FileTree.Tree.Nodes;
using JetBrains.Annotations;
using Warcraft.Core;

namespace FileTree.Tree.Serialized
{
    /// <summary>
    /// Represents a serialized node in a file tree. A node can be a directory or a file, and can have any number of child nodes.
    /// Furthermore, a node can be virtual, and be a "supernode" for other nodes. While not strictly enforced, it is
    /// expected that these subnodes have the same paths and names as the virtual node, and only differ in that they
    /// reside inside packages, while the virtual node is in a top-level tree.
    ///
    /// Whether or not a node is virtual depends on whether or not the flag <see cref="NodeType.Virtual"/> is set in
    /// <see cref="Type"/>.
    ///
    /// Typically, file nodes do not have any children, although it is not explicitly disallowed.
    /// </summary>
    [PublicAPI]
    public class SerializedNode
    {
        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        [PublicAPI]
        public NodeType Type { get; private set; }

        /// <summary>
        /// Gets the type of the file or directory pointed to by the node.
        /// </summary>
        [PublicAPI]
        public WarcraftFileType FileType { get; private set; }

        /// <summary>
        /// Gets the absolute offset where the name of this node is found. A negative value denotes no name.
        /// </summary>
        [PublicAPI]
        public long NameOffset { get; private set; }

        /// <summary>
        /// Gets the absolute offset to the parent node of this node. A negative value denotes no parent, and is reserved for
        /// the root node.
        /// </summary>
        [PublicAPI]
        public long ParentOffset { get; private set; }

        /// <summary>
        /// Gets the number of child nodes this node has.
        /// </summary>
        [PublicAPI]
        public ulong ChildCount { get; private set; }

        /// <summary>
        /// Gets a list of absolute offsets to where the children of this node can be found. This is comes in no particular
        /// enforced order - it is up to the consuming software to order them as necessary.
        /// </summary>
        [PublicAPI, NotNull]
        public List<ulong> ChildOffsets { get; } = new List<ulong>();

        /// <summary>
        /// Gets the number of hard nodes that this node has. This is only > 0 if <see cref="Type"/> is flagged
        /// as <see cref="NodeType.Virtual"/>.
        /// </summary>
        [PublicAPI]
        public ulong HardNodeCount { get; private set; }

        /// <summary>
        /// Gets a list of absolute offsets to where the hard nodes of this virtual node can be found. This only contains
        /// data if <see cref="Type"/> is flagged as <see cref="NodeType.Virtual"/>.
        /// </summary>
        [PublicAPI, NotNull]
        public List<ulong> HardNodeOffsets { get; } = new List<ulong>();

        /// <summary>
        /// Reads a new node from the specified <see cref="BinaryReader"/> at the specified position.
        /// </summary>
        /// <param name="br">The reader to read the node from.</param>
        /// <param name="position">The position in the reader to start reading at.</param>
        /// <returns>The node.</returns>
        [PublicAPI, NotNull]
        public static SerializedNode ReadNode([NotNull] BinaryReader br, ulong position)
        {
            br.BaseStream.Seek((long)position, SeekOrigin.Begin);

            var outNode = new SerializedNode
            {
                Type = (NodeType)br.ReadUInt32(),
                FileType = (WarcraftFileType)br.ReadUInt64(),
                NameOffset = br.ReadInt64(),
                ParentOffset = br.ReadInt64(),
                ChildCount = br.ReadUInt64()
            };

            for (ulong i = 0; i < outNode.ChildCount; ++i)
            {
                outNode.ChildOffsets.Add(br.ReadUInt64());
            }

            if (outNode.Type.HasFlag(NodeType.Virtual))
            {
                outNode.HardNodeCount = br.ReadUInt64();
                for (ulong i = 0; i < outNode.HardNodeCount; ++i)
                {
                    outNode.HardNodeOffsets.Add(br.ReadUInt64());
                }
            }

            return outNode;
        }

        /// <summary>
        /// Determines whether or not this node has children.
        /// </summary>
        /// <returns>true if the node has children; otherwise, false.</returns>
        [PublicAPI]
        public bool HasChildren()
        {
            return ChildCount > 0;
        }
    }
}
