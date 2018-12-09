using System.IO;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using FileTree.Tree;
using FileTree.Tree.Nodes;
using liblistfile;
using Moq;
using Warcraft.MPQ;

namespace FileTree.Benchmark
{
    public class Benchmark
    {
        private IPackage SamplePackage { get; set; }

        private ListfileDictionary Dictionary { get; set; }

        private TreeOptimizer _optimizer;

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

        [Benchmark]
        public Node NewAlgorithm()
        {
            var builder = new TreeBuilder();

            builder.AddPackage("sample-data", SamplePackage);
            return _optimizer.OptimizeTree(builder.GetTree());
        }
    }
}
