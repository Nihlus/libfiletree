//
//  EnumerableSpan.cs
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

namespace FileTree.Utilities.Span
{
    /// <summary>
    /// Represents an enumerable span.
    /// </summary>
    public ref struct EnumerableSpan
    {
        private readonly ReadOnlySpan<char> _span;
        private readonly char _delimiter;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableSpan"/> struct.
        /// </summary>
        /// <param name="span">The span.</param>
        /// <param name="delimiter">The delimiter.</param>
        public EnumerableSpan(ReadOnlySpan<char> span, char delimiter)
        {
            _span = span;
            _delimiter = delimiter;
        }

        /// <summary>
        /// Gets the enumerator of the span.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public SpanSplitEnumerator GetEnumerator() => new SpanSplitEnumerator(_span, _delimiter);
    }
}
