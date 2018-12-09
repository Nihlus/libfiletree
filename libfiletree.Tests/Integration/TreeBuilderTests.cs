//
//  TreeBuilderTests.cs
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

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileTree.Tests.Data;
using FileTree.Tree;
using FileTree.Tree.Nodes;
using FileTree.Tree.Serialized;
using liblistfile;
using Warcraft.Core;
using Warcraft.MPQ;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace FileTree.Tests.Integration
{
    public class TreeBuilderTests
    {
        private readonly TreeBuilder _treeBuilder;

        private readonly IPackage _package1;
        private readonly IPackage _package2;
        private readonly IPackage _emptyPackage;
        private readonly IPackage _singleFile;
        private readonly IPackage _singleFileInSubdir;

        private ListfileDictionary _dictionary;

        public TreeBuilderTests()
        {
            _treeBuilder = new TreeBuilder();

            _package1 = MockedPackageBuilder.GetMockedPackage("package1");
            _package2 = MockedPackageBuilder.GetMockedPackage("package2");

            _emptyPackage = MockedPackageBuilder.GetMockedPackage("empty-package");
            _singleFile = MockedPackageBuilder.GetMockedPackage("single-file");
            _singleFileInSubdir = MockedPackageBuilder.GetMockedPackage("single-file-in-subdir");

            var dictionaryData = File.ReadAllBytes("/home/jarl/.nuget/packages/liblistfile/2.1.0/contentFiles/any/netstandard2.0/Dictionary/dictionary.dic");
            _dictionary = new ListfileDictionary(dictionaryData);
        }

        [Fact]
        public void RootNodeHasNoName()
        {
            Assert.Empty(_treeBuilder.GetTree().Name);
        }

        [Fact]
        public void RootNodeIsMetaNode()
        {
            Assert.True(_treeBuilder.GetTree().Type.HasFlag(NodeType.Meta));
        }

        [Fact]
        public void RootNodeStartsWithOneChild()
        {
            var tree = _treeBuilder.GetTree();

            Assert.Single(tree.Children);
        }

        [Fact]
        public void FirstChildOfRootNodeIsMetaPackagesNode()
        {
            MetaPackagesNodeIsNode();
            MetaPackagesNodeIsNamedCorrectly();
            MetaPackagesNodeIsMetaNode();
            MetaPackagesNodeStartsWithoutChildren();
        }

        [Fact]
        public void MetaPackagesNodeIsNode()
        {
            var tree = _treeBuilder.GetTree();

            Assert.IsType<Node>(tree.Children.First());
        }

        [Fact]
        public void MetaPackagesNodeIsNamedCorrectly()
        {
            var tree = _treeBuilder.GetTree();
            var metaPackages = tree.Children.First();

            Assert.Equal("Packages", metaPackages.Name);
        }

        [Fact]
        public void MetaPackagesNodeIsMetaNode()
        {
            var tree = _treeBuilder.GetTree();
            var metaPackages = tree.Children.First();

            Assert.True(metaPackages.Type.HasFlag(NodeType.Meta));
        }

        [Fact]
        public void MetaPackagesNodeStartsWithoutChildren()
        {
            var tree = _treeBuilder.GetTree();
            var metaPackages = tree.Children.First();

            Assert.Empty(metaPackages.Children);
        }

        [Fact]
        public void AddingEmptyPackageCreatesEmptyPackageNode()
        {
            _treeBuilder.AddPackage("empty-package", _emptyPackage);

            var tree = _treeBuilder.GetTree();
            var metaPackages = tree.Children.First();

            Assert.Single(metaPackages.Children);

            var packageNode = metaPackages.Children.First();

            Assert.Equal("empty-package", packageNode.Name);

            Assert.Empty(packageNode.Children);

            Assert.True(packageNode.Type.HasFlag(NodeType.Meta));
            Assert.True(packageNode.Type.HasFlag(NodeType.Package));
        }

        [Fact]
        public void PackageWithSingleFileProducesNodeAtPackageRoot()
        {
            _treeBuilder.AddPackage("single-file", _singleFile);

            var tree = _treeBuilder.GetTree();
            var metaPackages = tree.Children.First();
            var packageNode = metaPackages.Children.First();

            var leaf = packageNode.Children.First();
            Assert.IsType<Node>(leaf);

            var fileNode = leaf;

            Assert.Equal("file.txt", fileNode.Name);
            Assert.True(fileNode.Type.HasFlag(NodeType.File));
            Assert.True(fileNode.FileType.HasFlag(WarcraftFileType.Text));
        }

        [Fact]
        public void PackageWithSingleFileProducesVirtualNodeAtRoot()
        {
            _treeBuilder.AddPackage("single-file", _singleFile);

            var tree = _treeBuilder.GetTree();
            Assert.Equal(2, tree.Children.Count);

            var leaf = tree.Children[1];
            Assert.IsType<VirtualNode>(leaf);

            var fileNode = (VirtualNode)leaf;

            Assert.Equal("file.txt", fileNode.Name);
            Assert.True(fileNode.Type.HasFlag(NodeType.File));
            Assert.True(fileNode.FileType.HasFlag(WarcraftFileType.Text));
        }

        [Fact]
        public void PackageWithSingleFileProducesNodeAtRootWithOneHardNode()
        {
            _treeBuilder.AddPackage("single-file", _singleFile);

            var tree = _treeBuilder.GetTree();
            var leaf = tree.Children[1];
            var fileNode = (VirtualNode)leaf;

            Assert.Single(fileNode.HardNodes);
        }

        [Fact]
        public void PackageWithSingleFileProducesNodeAtRootWithCorrectHardNode()
        {
            _treeBuilder.AddPackage("single-file", _singleFile);

            var tree = _treeBuilder.GetTree();

            var metaPackages = tree.Children.First();
            var packageNode = metaPackages.Children.First();

            var leaf = tree.Children[1];
            var fileNode = (VirtualNode)leaf;

            var referencedHardNode = fileNode.HardNodes.First();
            var actualHardNode = packageNode.Children.First();

            Assert.Same(actualHardNode, referencedHardNode);
        }

        [Fact]
        public void PackageWithSingleFileInSubdirectoryProducesVirtualNodeAtRoot()
        {
            _treeBuilder.AddPackage("single-file-in-subdir", _singleFileInSubdir);

            var tree = _treeBuilder.GetTree();

            var directoryNode = (VirtualNode)tree.Children[1];

            Assert.Equal("Assets", directoryNode.Name);
            Assert.True(directoryNode.Type.HasFlag(NodeType.Directory));
        }

        [Fact]
        public void PackageWithSingleFileInSubdirectoryProducesNodeWithNodeChild()
        {
            _treeBuilder.AddPackage("single-file-in-subdir", _singleFileInSubdir);

            var tree = _treeBuilder.GetTree();

            var branch = tree.Children[1];
            var leaf = branch.Children.First();

            Assert.Equal("file.txt", leaf.Name);
            Assert.True(leaf.Type.HasFlag(NodeType.File));
        }

        [Fact]
        public void PackageWithSingleFileInSubdirectoryProducesNodeAtPackageRoot()
        {
            _treeBuilder.AddPackage("single-file-in-subdir", _singleFileInSubdir);

            var tree = _treeBuilder.GetTree();

            var metaPackages = tree.Children.First();
            var packageNode = metaPackages.Children.First();

            var directoryNode = packageNode.Children[0];

            Assert.Equal("Assets", directoryNode.Name);
            Assert.True(directoryNode.Type.HasFlag(NodeType.Directory));
        }

        [Fact]
        public void PackageWithSingleFileInSubdirectoryProducesVirtualNodeWithVirtualNodeChild()
        {
            _treeBuilder.AddPackage("single-file-in-subdir", _singleFileInSubdir);

            var tree = _treeBuilder.GetTree();

            var metaPackages = tree.Children.First();
            var packageNode = metaPackages.Children.First();

            var branch = packageNode.Children[0];
            var leaf = branch.Children.First();

            Assert.Equal("file.txt", leaf.Name);
            Assert.True(leaf.Type.HasFlag(NodeType.File));
        }

        [Fact]
        public void PackageWithSingleFileInSubdirectoryProducesVirtualNodeAtRootWithCorrectHardNode()
        {
            _treeBuilder.AddPackage("single-file-in-subdir", _singleFileInSubdir);

            var tree = _treeBuilder.GetTree();

            var directoryNode = (VirtualNode)tree.Children[1];

            var metaPackages = tree.Children.First();
            var packageNode = metaPackages.Children.First();

            var hardDirectoryNode = packageNode.Children[0];

            var actualHardNode = directoryNode.HardNodes.First();

            var expectedHardNode = hardDirectoryNode;

            Assert.Same(expectedHardNode, actualHardNode);
        }

        [Fact]
        public void AddingMultiplePackagesWithSamePathProducesMultipleHardNodesWhichOneVirtualNodeMapsTo()
        {
            _treeBuilder.AddPackage("package1", _package1);
            _treeBuilder.AddPackage("package2", _package2);

            var tree = _treeBuilder.GetTree();

            var virtualNode = (VirtualNode)tree.Children.First(c => c.Name == "Textures");

            var metaPackages = tree.Children.First();

            var package1Node = metaPackages.Children.First(c => c.Name == "package1");
            var package2Node = metaPackages.Children.First(c => c.Name == "package2");

            var hardNode1 = package1Node.Children.First(c => c.Name == "Textures");
            var hardNode2 = package2Node.Children.First(c => c.Name == "Textures");

            Assert.True(virtualNode.HardNodes.Contains(hardNode1));
            Assert.True(virtualNode.HardNodes.Contains(hardNode2));
        }

        [Fact]
        public async Task CanSerializeTree()
        {
            _treeBuilder.AddPackage("package1", _package1);

            var tree = _treeBuilder.GetTree();
            var optimizer = new TreeOptimizer(_dictionary);

            tree = optimizer.OptimizeTree(tree);

            using (var ms = new MemoryStream())
            {
                using (var serializer = new TreeSerializer(ms, true))
                {
                    await serializer.SerializeAsync(tree);
                }

                // Rewind the stream
                ms.Position = 0;

                var optimizedTree = new SerializedTree(ms);

                Assert.Equal((ulong)tree.Children.Count, optimizedTree.Root.ChildCount);

                var rootChildren = optimizedTree.Root.ChildOffsets.Select(optimizedTree.GetNode);
                var rootChildNames = rootChildren.Select(optimizedTree.GetNodeName).ToList();

                Assert.Contains(rootChildNames, n => n == "Textures");
                Assert.Contains(rootChildNames, n => n == "rootfile.txt");
                Assert.Contains(rootChildNames, n => n == "file-to-be-deleted.txt");
            }
        }
    }
}
