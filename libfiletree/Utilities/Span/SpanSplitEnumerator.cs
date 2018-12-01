//
//  SpanSplitEnumerator.cs
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
    /// Enumerator class for splitting a span by an input character.
    /// </summary>
    public ref struct SpanSplitEnumerator
    {
        private ReadOnlySpan<char> _span;
        private char _delimiter;

        /// <summary>
        /// Gets the current part.
        /// </summary>
        public ReadOnlySpan<char> Current { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpanSplitEnumerator"/> struct.
        /// </summary>
        /// <param name="span">The span to split.</param>
        /// <param name="delimiter">The delimiter to split against.</param>
        public SpanSplitEnumerator(ReadOnlySpan<char> span, char delimiter)
        {
            _span = span;
            _delimiter = delimiter;

            Current = ReadOnlySpan<char>.Empty;
        }

        /// <summary>
        /// Moves the enumerator to the next part of the span.
        /// </summary>
        /// <returns>true if the enumerator advanced; otherwise, false.</returns>
        public bool MoveNext()
        {
            if (_span.Length == 0)
            {
                return false;
            }

            int sliceStart = 0;
            int sliceEnd = 0;

            while (sliceEnd < _span.Length && _span[sliceEnd] != '\\')
            {
                ++sliceEnd;
            }

            var part = _span.Slice(sliceStart, sliceEnd - sliceStart);
            if (sliceEnd == _span.Length)
            {
                _span = _span.Slice(sliceEnd);
            }
            else
            {
                // Skip the path separator char
                _span = _span.Slice(sliceEnd + 1);
            }

            Current = part;

            return true;
        }
    }
}
