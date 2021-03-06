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
using System.IO;
using System.Linq;
using System.Threading;
using FileTree.ProgressReporters;
using FileTree.Tree.Nodes;
using JetBrains.Annotations;
using Warcraft.Core;
using Warcraft.MPQ;

namespace FileTree.Tree
{
    /// <summary>
    /// Builds node trees from input packages.
    /// </summary>
    [PublicAPI]
    public class TreeBuilder
    {
        /// <summary>
        /// Gets the root node of the tree.
        /// </summary>
        [NotNull]
        private Node Root { get; }

        /// <summary>
        /// Gets the meta node under which separate package trees are kept.
        /// </summary>
        [NotNull]
        private Node PackagesMetaNode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeBuilder"/> class.
        /// </summary>
        [PublicAPI]
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
        [PublicAPI]
        public void AddPackage([NotNull] string packageName, [NotNull] IPackage package)
            => AddPackage(packageName, package, null, CancellationToken.None);

        /// <summary>
        /// Adds the file list of the given package to the node tree.
        /// </summary>
        /// <param name="packageName">The name of the package.</param>
        /// <param name="package">The package.</param>
        /// <param name="ct">The cancellation token to use.</param>
        [PublicAPI]
        public void AddPackage([NotNull] string packageName, [NotNull] IPackage package, CancellationToken ct)
            => AddPackage(packageName, package, null, ct);

        /// <summary>
        /// Adds the file list of the given package to the node tree.
        /// </summary>
        /// <param name="packageName">The name of the package.</param>
        /// <param name="package">The package.</param>
        /// <param name="progress">The progress reporter to use.</param>
        /// <param name="ct">The cancellation token to use.</param>
        [PublicAPI]
        public void AddPackage
        (
            [NotNull] string packageName,
            [NotNull] IPackage package,
            [CanBeNull] IProgress<PackageNodesCreationProgress> progress,
            CancellationToken ct = default
        )
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

            var paths = package.GetFileList().ToList();

            // This loop iterates over each path in turn, forming a left-to-right scanning ReadOnlySpan<char> that picks
            // out the individual path components without allocating additional strings
            ulong completedPaths = 0;
            var progressReport = new PackageNodesCreationProgress
            {
                PathCount = (ulong)paths.Count
            };

            foreach (var path in paths)
            {
                ct.ThrowIfCancellationRequested();

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
                        if (!childVirtualNode.Name.AsSpan().Equals(part, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        existingVirtualNode = childVirtualNode;
                        break;
                    }

                    Node existingHardNode = null;
                    foreach (var childHardNode in parentHardNode.Children)
                    {
                        if (!childHardNode.Name.AsSpan().Equals(part, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        existingHardNode = childHardNode;
                        break;
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

                            var fileInfo = package.GetFileInfo(path);

                            if (!(fileInfo is null) && fileInfo.IsDeleted)
                            {
                                existingHardNode.Type |= NodeType.Deleted;
                            }
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

                    if (pathSpan.Length == 0)
                    {
                        continue;
                    }

                    parentVirtualNode = existingVirtualNode;
                    parentHardNode = existingHardNode;
                }

                if (progress is null)
                {
                    continue;
                }

                completedPaths++;
                progressReport.CompletedPaths = completedPaths;

                progress.Report(progressReport);
            }
        }

        /// <summary>
        /// Gets the tree contained in the builder.
        /// </summary>
        /// <returns>The root node of the tree.</returns>
        [PublicAPI, NotNull]
        public Node GetTree()
        {
            return Root;
        }
    }
}
