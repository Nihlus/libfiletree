//
//  StringlikeSpanExtensions.cs
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
using FileTree.Utilities.Span;

namespace FileTree.Extensions
{
    /// <summary>
    /// Holds extension methods for stringlike spans.
    /// </summary>
    public static class StringlikeSpanExtensions
    {
        /// <summary>
        /// Splits a span based on the given delimiter.
        /// </summary>
        /// <param name="span">The span to split.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns>An enumerator that can be iterated over.</returns>
        public static EnumerableSpan Split(this ReadOnlySpan<char> span, char delimiter)
        {
            return new EnumerableSpan(span, delimiter);
        }
    }
}
