//
//  TreeBuilder.cs
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
using System.Linq;
using FileTree.Tree.Nodes;
using JetBrains.Annotations;
using Warcraft.Core;
using Warcraft.MPQ;

namespace FileTree.Tree
{
    /// <summary>
    /// Builds node trees from input packages.
    /// </summary>
    public class TreeBuilder
    {
        /// <summary>
        /// Gets the root node of the tree.
        /// </summary>
        private Node Root { get; }

        /// <summary>
        /// Gets the meta node under which separate package trees are kept.
        /// </summary>
        private Node PackagesMetaNode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeBuilder"/> class.
        /// </summary>
        public TreeBuilder()
        {
            Root = new Node
            {
                Name = string.Empty,
                Type = NodeType.Meta
            };

            PackagesMetaNode = new Node
            {
                Name = "Packages",
                Type = NodeType.Meta
            };

            Root.AppendChild(PackagesMetaNode);
        }

        /// <summary>
        /// Adds the file list of the given package to the node tree.
        /// </summary>
        /// <param name="packageName">The name of the package.</param>
        /// <param name="package">The package.</param>
        public void AddPackage(string packageName, [NotNull] IPackage package)
        {
            if (PackagesMetaNode.Children.Any(c => c.Name == packageName))
            {
                throw new ArgumentException("A package with that name has already been added.");
            }

            var packageNode = new Node
            {
                Name = packageName,
                Type = NodeType.Meta | NodeType.Package
            };

            PackagesMetaNode.AppendChild(packageNode);

            // This loop iterates over each path in turn, forming a left-to-right scanning ReadOnlySpan<char> that picks
            // out the individual path components without
            foreach (var path in package.GetFileList())
            {
                IBranchNode parentVirtualNode = Root;
                Node parentHardNode = packageNode;

                var pathSpan = path.AsSpan();

                while (pathSpan.Length > 0)
                {
                    int sliceStart = 0;
                    int sliceEnd = 0;

                    while (sliceEnd < pathSpan.Length && pathSpan[sliceEnd] != '\\')
                    {
                        ++sliceEnd;
                    }

                    var part = pathSpan.Slice(sliceStart, sliceEnd - sliceStart);
                    if (sliceEnd == pathSpan.Length)
                    {
                        pathSpan = pathSpan.Slice(sliceEnd);
                    }
                    else
                    {
                        // Skip the path separator char
                        pathSpan = pathSpan.Slice(sliceEnd + 1);
                    }

                    Node existingVirtualNode = null;
                    foreach (var childVirtualNode in parentVirtualNode.Children)
                    {
                        if (childVirtualNode.Name.AsSpan().Equals(part, StringComparison.OrdinalIgnoreCase))
                        {
                            existingVirtualNode = childVirtualNode;
                            break;
                        }
                    }

                    Node existingHardNode = null;
                    foreach (var childHardNode in parentHardNode.Children)
                    {
                        if (childHardNode.Name.AsSpan().Equals(part, StringComparison.OrdinalIgnoreCase))
                        {
                            existingHardNode = childHardNode;
                            break;
                        }
                    }

                    if (!(existingVirtualNode is null) && !(existingHardNode is null))
                    {
                        parentHardNode = existingHardNode;
                        parentVirtualNode = existingVirtualNode;

                        continue;
                    }

                    var partName = part.ToString();

                    if (existingHardNode is null)
                    {
                        if (pathSpan.Length == 0)
                        {
                            existingHardNode = new Node
                            {
                                Name = partName,
                                FileType = FileInfoUtilities.GetFileType(partName),
                                Type = NodeType.File
                            };
                        }
                        else
                        {
                            existingHardNode = new Node
                            {
                                Name = partName,
                                Type = NodeType.Directory
                            };
                        }

                        parentHardNode.AppendChild(existingHardNode);
                    }

                    if (existingVirtualNode is null)
                    {
                        if (pathSpan.Length == 0)
                        {
                            var newVirtualNode = new VirtualNode
                            {
                                Name = partName,
                                FileType = FileInfoUtilities.GetFileType(partName),
                                Type = NodeType.Virtual | NodeType.File
                            };

                            newVirtualNode.AppendHardNode(existingHardNode);

                            existingVirtualNode = newVirtualNode;
                        }
                        else
                        {
                            var newVirtualNode = new VirtualNode
                            {
                                Name = partName,
                                Type = NodeType.Virtual | NodeType.Directory
                            };

                            newVirtualNode.AppendHardNode(existingHardNode);
                            existingVirtualNode = newVirtualNode;
                        }

                        parentVirtualNode.AppendChild(existingVirtualNode);
                    }
                    else
                    {
                        if (existingVirtualNode is VirtualNode virtualNode)
                        {
                            virtualNode.AppendHardNode(existingHardNode);
                        }
                    }

                    if (pathSpan.Length != 0)
                    {
                        parentVirtualNode = existingVirtualNode;
                        parentHardNode = existingHardNode;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the tree contained in the builder.
        /// </summary>
        /// <returns>The root node of the tree.</returns>
        public Node GetTree()
        {
            return Root;
        }
    }
}
