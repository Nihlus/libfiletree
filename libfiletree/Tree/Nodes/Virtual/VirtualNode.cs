//
//  VirtualNode.cs
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

namespace FileTree.Tree.Nodes
{
    /// <summary>
    /// Represents a virtual branch node that has one or more "hard" nodes it acts as a front for.
    /// </summary>
    [PublicAPI]
    public class VirtualNode : Node, IVirtualNode
    {
        /// <inheritdoc />
        public IList<Node> HardNodes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNode"/> class.
        /// </summary>
        [PublicAPI]
        public VirtualNode()
        {
            HardNodes = new List<Node>();
        }

        /// <inheritdoc />
        public void AppendHardNode(Node hardNode)
        {
            HardNodes.Add(hardNode);
        }
    }
}
