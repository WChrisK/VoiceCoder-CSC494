//  An exception indicating something went wrong when compiling.
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
    /// Occurs if a compiler cannot compile a stream of tokens.
    /// </summary>
    public class CompilerException : ParserException
    {

        /// <summary>
        /// Initializes a new compiler exception.
        /// </summary>
        public CompilerException()
        {
        }

        /// <summary>
        /// Initializes a new compiler exception with a message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public CompilerException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new compiler exception with a message and a cause.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="inner">The cause of this exception.</param>
        public CompilerException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
