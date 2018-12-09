//
//  Program.cs
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
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;

namespace FileTree.Benchmark
{
    /// <summary>
    /// The main program class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The main entry point.
        /// </summary>
        /// <param name="args">The arguments passed to the application.</param>
        public static void Main(string[] args)
        {
            var config = ManualConfig.Create(DefaultConfig.Instance)
                .With
                (
                    new SimpleFilter
                    (
                        b =>
                        {
                            var isClrJob = b.Job.Environment.Runtime?.Name == "Clr";
                            var isRunningOnMono = !(Type.GetType("Mono.Runtime") is null);

                            if (!isClrJob)
                            {
                                return true;
                            }

                            return !isRunningOnMono;
                        }
                    )
                );

            BenchmarkRunner.Run<Benchmark>(config);
        }
    }
}
