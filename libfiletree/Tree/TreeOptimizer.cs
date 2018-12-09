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

using FileTree.Tree.Nodes;
using JetBrains.Annotations;
using liblistfile;
using Warcraft.Core;

namespace FileTree.Tree
{
    /// <summary>
    /// Optimizes a node tree, normalizing names and applying file type traces in branch nodes.
    /// </summary>
    public class TreeOptimizer
    {
        private readonly ListfileDictionary _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeOptimizer"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary to use.</param>
        public TreeOptimizer(ListfileDictionary dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>
        /// Optimizes the given tree.
        /// </summary>
        /// <param name="root">The root node of the tree.</param>
        /// <returns>The optimized tree.</returns>
        public Node OptimizeTree(Node root)
        {
            var tree = root;

            tree = NormalizeNames(tree);
            tree = ApplyFileTraces(tree);

            return tree;
        }

        /// <summary>
        /// Normalizes the names in tree according to the dictionary.
        /// </summary>
        /// <param name="tree">The root node of the tree to normalize.</param>
        /// <returns>The normalized tree.</returns>
        [NotNull]
        private Node NormalizeNames([NotNull] Node tree)
        {
            NormalizeNode(tree);

            return tree;
        }

        /// <summary>
        /// Normalizes the names in node according to the dictionary.
        /// </summary>
        /// <param name="node">The node to normalize.</param>
        private void NormalizeNode([NotNull] Node node)
        {
            if (!node.Type.HasFlag(NodeType.Meta))
            {
                var entry = _dictionary.GetTermEntry(node.Name);
                if (!(entry is null))
                {
                    node.Name = _dictionary.GetTermEntry(node.Name).Term;
                }
            }

            foreach (var child in node.Children)
            {
                NormalizeNode(child);
            }
        }

        /// <summary>
        /// Applies file traces in the tree, giving branch nodes the same file type as their contained files.
        /// </summary>
        /// <param name="root">The root node to apply traces to.</param>
        /// <returns>The traced tree.</returns>
        [NotNull]
        private Node ApplyFileTraces([NotNull] Node root)
        {
            foreach (var child in root.Children)
            {
                root.FileType |= GetFileTypes(child);
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
