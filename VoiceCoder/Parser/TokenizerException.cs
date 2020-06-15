//  An exception indicating something went wrong when tokenizing data.
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

namespace VoiceCoder.Parser
{
    /// <summary>
    /// Occurs if a tokenizer runs into any unexpected tokenizing errors.
    /// </summary>
    public class TokenizerException : ParserException
    {
        /// <summary>
        /// The line number this exception occured at.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// The offset on the line that this exception occured at.
        /// </summary>
        public int CharOffset { get; }

        /// <summary>
        /// Initializes a new tokenizer exception.
        /// </summary>
        /// <param name="lineNumber">The line number this occured at.</param>
        /// <param name="charOffset">The character offset for the line this
        /// occured at.</param>
        public TokenizerException(int lineNumber, int charOffset)
        {
            LineNumber = lineNumber;
            CharOffset = charOffset;
        }

        /// <summary>
        /// Initializes a new tokenizer exception with a message.
        /// </summary>
        /// <param name="lineNumber">The line number this occured at.</param>
        /// <param name="charOffset">The character offset for the line this
        /// occured at.</param>
        /// <param name="message">The message for the exception.</param>
        public TokenizerException(int lineNumber, int charOffset, string message) : base(message)
        {
            LineNumber = lineNumber;
            CharOffset = charOffset;
        }

        /// <summary>
        /// Initializes a new tokenizer exception with a message and a cause.
        /// </summary>
        /// <param name="lineNumber">The line number this occured at.</param>
        /// <param name="charOffset">The character offset for the line this
        /// occured at.</param>
        /// <param name="message">The message for the exception.</param>
        /// <param name="inner">The cause of this exception.</param>
        public TokenizerException(int lineNumber, int charOffset, string message, Exception inner) 
            : base(message, inner)
        {
            LineNumber = lineNumber;
            CharOffset = charOffset;
        }
    }
}
