//
//  TreeOptimizationProgress.cs
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

using JetBrains.Annotations;

namespace FileTree.ProgressReporters
{
    /// <summary>
    /// Holds information related to the progress of adding a package to a node tree.
    /// </summary>
    [PublicAPI]
    public class TreeOptimizationProgress
    {
        /// <summary>
        /// Gets or sets the total number of nodes in the tree that are being optimized.
        /// </summary>
        [PublicAPI]
        public ulong NodeCount { get; set; }

        /// <summary>
        /// Gets or sets the number of nodes that have been optimized.
        /// </summary>
        [PublicAPI]
        public ulong OptimizedNodes { get; set; }

        /// <summary>
        /// Gets or sets the number of nodes that have been given their file traces.
        /// </summary>
        [PublicAPI]
        public ulong TracedNodes { get; set; }
    }
}
