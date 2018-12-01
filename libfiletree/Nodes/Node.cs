//
//  Node.cs
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
using JetBrains.Annotations;
using Warcraft.Core;

namespace FileTree.Nodes
{
    /// <summary>
    /// Represents a node in a file tree.
    /// </summary>
    public class Node : IBranchNode
    {
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        [NotNull]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the node.
        /// </summary>
        public NodeType Type { get; set; }

        /// <summary>
        /// Gets or sets the file type of the leaf node.
        /// </summary>
        public WarcraftFileType FileType { get; set; }

        /// <summary>
        /// Gets or sets the node's parent.
        /// </summary>
        [CanBeNull]
        public Node Parent { get; set; }

        /// <inheritdoc />
        public IList<Node> Children { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        public Node()
        {
            Children = new List<Node>();
        }

        /// <inheritdoc />
        public void AppendChild(Node child)
        {
            Children.Add(child);
            child.Parent = this;
        }
    }
}