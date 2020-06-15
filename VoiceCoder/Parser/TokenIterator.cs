//  Helps iterate over a set of tokens (for the compiler).
//  Copyright(C) 2016  Chris K
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Collections.Generic;
using static VoiceCoder.Util.Assertion;

namespace VoiceCoder.Parser
{
    /// <summary>
    /// Wraps around a list of tokens and provides convenience methods for
    /// compiling the tokens.
    /// </summary>
    public class TokenIterator
    {
        /// <summary>
        /// The index that of the token list that will be returned on the next
        /// call.
        /// </summary>
        private int index;

        /// <summary>
        /// The list of tokens to iterate over.
        /// </summary>
        private List<Token> tokens;

        /// <summary>
        /// Gets the number of tokens this iterates over.
        /// </summary>
        public int Count { get { return tokens.Count; } }

        /// <summary>
        /// Creates a token iterator that wraps around the provided list of
        /// tokens. This list should not be modified or else the behavior is
        /// undefined.
        /// </summary>
        /// <param name="tokens"></param>
        public TokenIterator(List<Token> tokens)
        {
            CheckNotNull(tokens);
            this.tokens = tokens;
        }

        /// <summary>
        /// Checks if there's another token available.
        /// </summary>
        /// <returns>True if there's another token, false if not.</returns>
        public bool HasNext()
        {
            return index < tokens.Count;
        }

        /// <summary>
        /// Checks if the next token type is of the provided types.
        /// </summary>
        /// <param name="types">The types to check.</param>
        /// <returns>True if it is, false if not or if there is no next token.
        /// </returns>
        public bool HasNextType(params TokenType[] types)
        {
            if (!HasNext())
            {
                return false;
            }

            return types.Contains(tokens[index].Type);
        }

        /// <summary>
        /// Gets the next token.
        /// </summary>
        /// <returns>The next token.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If there is not any
        /// tokens left to get.</exception>
        public Token Next()
        {
            if (index >= tokens.Count)
            {
                throw new ArgumentOutOfRangeException("TokenIterator is out of range.");
            }

            Token token = tokens[index];
            index++;
            return token;
        }

        /// <summary>
        /// Gets a marker for the current token location. Can be combined with
        /// SetToMarker to rewind to the location.
        /// </summary>
        /// <returns>A marker to the current location.</returns>
        public int GetMarker()
        {
            return index;
        }

        /// <summary>
        /// Sets the marker to the marked location returned from GetMarker.
        /// This does not work on an empty token iterator.
        /// </summary>
        /// <param name="marker">The marker to set to.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the marker index
        /// provided is negative or out of range of the token array size.
        /// </exception>
        public void SetToMarker(int marker)
        {
            if (marker < 0)
            {
                throw new ArgumentOutOfRangeException("TokenIterator marker was negative.");
            }
            else if (marker >= tokens.Count)
            {
                throw new ArgumentOutOfRangeException($"TokenIterator marker is out of range: {index} >= {tokens.Count}");
            }

            index = marker;
        }

        /// <summary>
        /// Resets the iterator to the beginning. This is a convenience method
        /// that is identical to setting the marker to the beginning.
        /// </summary>
        public void ResetToBeginning()
        {
            SetToMarker(0);
        }
    }
}
