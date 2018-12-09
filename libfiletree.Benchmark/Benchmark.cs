//
//  Benchmark.cs
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
using System.Reflection;
using BenchmarkDotNet.Attributes;
using FileTree.Tree;
using FileTree.Tree.Nodes;
using JetBrains.Annotations;
using ListFile;
using Moq;
using Warcraft.MPQ;

namespace FileTree.Benchmark
{
    /// <summary>
    /// Contains benchmarking setup.
    /// </summary>
    public class Benchmark
    {
        private IPackage SamplePackage { get; set; }

        private ListfileDictionary Dictionary { get; set; }

        private TreeOptimizer _optimizer;

        /// <summary>
        /// Sets up shared data for benchmarks.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            string[] fileList;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FileTree.Benchmark.Data.big-sample-data.txt"))
            {
                using (var sr = new StreamReader(stream))
                {
                    fileList = sr.ReadToEnd().Split('\n');
                }
            }

            var mockedPackage = new Mock<IPackage>();
            mockedPackage.Setup(p => p.HasFileList()).Returns(true);
            mockedPackage.Setup(p => p.GetFileList()).Returns(fileList);

            SamplePackage = mockedPackage.Object;

            var dictionaryData = File.ReadAllBytes("/home/jarl/.nuget/packages/liblistfile/2.1.0/contentFiles/any/netstandard2.0/Dictionary/dictionary.dic");
            Dictionary = new ListfileDictionary(dictionaryData);

            _optimizer = new TreeOptimizer(Dictionary);
        }

        /// <summary>
        /// Benchmarks the new algorithm.
        /// </summary>
        /// <returns>The generated node.</returns>
        [Benchmark, NotNull]
        public Node NewAlgorithm()
        {
            var builder = new TreeBuilder();

            builder.AddPackage("sample-data", SamplePackage);
            return _optimizer.OptimizeTree(builder.GetTree());
        }
    }
}
