//
//  MockedPackageBuilder.cs
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
using System.Reflection;
using Moq;
using Warcraft.MPQ;

namespace FileTree.Tests.Data
{
    /// <summary>
    /// Builds mocked packages.
    /// </summary>
    public static class MockedPackageBuilder
    {
        /// <summary>
        /// Gets a mocked package, using the named file list (stored as an embedded resource).
        /// </summary>
        /// <param name="fileList">The name of the file list.</param>
        /// <returns>The mocked package.</returns>
        public static IPackage GetMockedPackage(string fileList)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string[] files;
            using (var rs = assembly.GetManifestResourceStream($"FileTree.Tests.Data.FileLists.{fileList}.txt"))
            {
                using (var sr = new StreamReader(rs))
                {
                    files = sr.ReadToEnd().Split('\n');
                }
            }

            var mockedPackage = new Mock<IPackage>();
            mockedPackage.Setup(p => p.GetFileList()).Returns(files.ToList());
            mockedPackage.Setup(p => p.HasFileList()).Returns(true);

            return mockedPackage.Object;
        }
    }
}
