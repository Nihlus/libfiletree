//
//  IVirtualNode.cs
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
    /// Represents the public interface of a virtual node.
    /// </summary>
    [PublicAPI]
    public interface IVirtualNode
    {
        /// <summary>
        /// Gets the hard nodes that this virtual node represents.
        /// </summary>
        [PublicAPI, NotNull, ItemNotNull]
        IList<Node> HardNodes { get; }

        /// <summary>
        /// Appends a hard node to this virtual node.
        /// </summary>
        /// <param name="hardNode">The hard node.</param>
        [PublicAPI]
        void AppendHardNode([NotNull] Node hardNode);
    }
}
