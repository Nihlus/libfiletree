//
//  TreeOptimizer.cs
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
using System.Threading;
using FileTree.ProgressReporters;
using FileTree.Tree.Nodes;
using JetBrains.Annotations;
using ListFile;
using Warcraft.Core;

namespace FileTree.Tree
{
    /// <summary>
    /// Optimizes a node tree, normalizing names and applying file type traces in branch nodes.
    /// </summary>
    [PublicAPI]
    public class TreeOptimizer
    {
        [NotNull]
        private readonly ListfileDictionary _dictionary;

        [NotNull]
        private readonly TreeOptimizationProgress _progressReport;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeOptimizer"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary to use.</param>
        [PublicAPI]
        public TreeOptimizer([NotNull] ListfileDictionary dictionary)
        {
            _dictionary = dictionary;
            _progressReport = new TreeOptimizationProgress();
        }

        /// <summary>
        /// Optimizes the given tree.
        /// </summary>
        /// <param name="root">The root node of the tree.</param>
        /// <returns>The optimized tree.</returns>
        [PublicAPI, NotNull]
        public Node OptimizeTree([NotNull] Node root)
            => OptimizeTree(root, null);

        /// <summary>
        /// Optimizes the given tree.
        /// </summary>
        /// <param name="root">The root node of the tree.</param>
        /// <param name="ct">The cancellation token to use.</param>
        /// <returns>The optimized tree.</returns>
        [PublicAPI, NotNull]
        public Node OptimizeTree([NotNull] Node root, CancellationToken ct)
            => OptimizeTree(root, null, ct);

        /// <summary>
        /// Optimizes the given tree.
        /// </summary>
        /// <param name="root">The root node of the tree.</param>
        /// <param name="progress">The progress reporter to use.</param>
        /// <param name="ct">The cancellation token to use.</param>
        /// <returns>The optimized tree.</returns>
        [PublicAPI, NotNull]
        public Node OptimizeTree
        (
            [NotNull] Node root,
            [CanBeNull] IProgress<TreeOptimizationProgress> progress,
            CancellationToken ct = default
        )
        {
            var tree = root;

            _progressReport.NodeCount = tree.CountChildren() + 1;

            ct.ThrowIfCancellationRequested();

            tree = NormalizeNames(tree, progress, ct);
            tree = ApplyFileTraces(tree, progress, ct);

            return tree;
        }

        /// <summary>
        /// Normalizes the names in tree according to the dictionary.
        /// </summary>
        /// <param name="tree">The root node of the tree to normalize.</param>
        /// <param name="progress">The progress reporter to use.</param>
        /// <param name="ct">The cancellation token to use.</param>
        /// <returns>The normalized tree.</returns>
        [PublicAPI, NotNull]
        private Node NormalizeNames
        (
            [NotNull] Node tree,
            [CanBeNull] IProgress<TreeOptimizationProgress> progress,
            CancellationToken ct
        )
        {
            NormalizeNode(tree, progress, ct);

            return tree;
        }

        /// <summary>
        /// Normalizes the names in node according to the dictionary.
        /// </summary>
        /// <param name="node">The node to normalize.</param>
        /// <param name="progress">The progress reporter to use.</param>
        /// <param name="ct">The cancellation token to use.</param>
        private void NormalizeNode
        (
            [NotNull] Node node,
            [CanBeNull] IProgress<TreeOptimizationProgress> progress,
            CancellationToken ct
        )
        {
            ct.ThrowIfCancellationRequested();

            if (!node.Type.HasFlag(NodeType.Meta))
            {
                if (_dictionary.ContainsTerm(node.Name))
                {
                    node.Name = _dictionary.GetTermEntry(node.Name).Term;
                }

                if (!(progress is null))
                {
                    _progressReport.OptimizedNodes++;
                    progress.Report(_progressReport);
                }
            }

            foreach (var child in node.Children)
            {
                NormalizeNode(child, progress, ct);
            }
        }

        /// <summary>
        /// Applies file traces in the tree, giving branch nodes the same file type as their contained files.
        /// </summary>
        /// <param name="root">The root node to apply traces to.</param>
        /// <param name="progress">The progress reporter to use.</param>
        /// <param name="ct">The cancellation token to use.</param>
        /// <returns>The traced tree.</returns>
        [NotNull]
        private Node ApplyFileTraces
        (
            [NotNull] Node root,
            [CanBeNull] IProgress<TreeOptimizationProgress> progress,
            CancellationToken ct
        )
        {
            foreach (var child in root.Children)
            {
                ct.ThrowIfCancellationRequested();

                root.FileType |= GetFileTypes(child);

                if (!(progress is null))
                {
                    _progressReport.TracedNodes++;
                    progress.Report(_progressReport);
                }
            }

            return root;
        }

        /// <summary>
        /// Gets the file types contained in the given node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The file types.</returns>
        private WarcraftFileType GetFileTypes([NotNull] Node node)
        {
            var types = node.FileType;
            foreach (var child in node.Children)
            {
                if (child.Type.HasFlag(NodeType.Meta))
                {
                    continue;
                }

                types |= GetFileTypes(child);
            }

            return types;
        }
    }
}
