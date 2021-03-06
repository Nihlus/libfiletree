//
//  IBranchNode.cs
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
    /// Represents the public interface of a branch node.
    /// </summary>
    [PublicAPI]
    public interface IBranchNode
    {
        /// <summary>
        /// Gets the children of the node.
        /// </summary>
        [PublicAPI, NotNull, ItemNotNull]
        IList<Node> Children { get; }

        /// <summary>
        /// Appends the given child to the branch node.
        /// </summary>
        /// <param name="child">The child to append.</param>
        [PublicAPI]
        void AppendChild([NotNull] Node child);
    }
}
