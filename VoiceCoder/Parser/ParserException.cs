//  A generic exception that is thrown by a Parser.
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
    /// Indicates that some kind of exception occured while parsing data.
    /// Designed to be a superclass for the different parsing exceptions
    /// that can occur (tokenizing, compiling, etc).
    /// </summary>
    public class ParserException : Exception
    {
        /// <summary>
        /// Initializes a new empty parser exception.
        /// </summary>
        public ParserException()
        {
        }

        /// <summary>
        /// Initializes a new parser exception with a message.
        /// </summary>
        /// <param name="message">The message for this exception.</param>
        public ParserException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new parser exception with a message and cause.
        /// </summary>
        /// <param name="message">The message for this exception.</param>
        /// <param name="inner">The cause of this exception.</param>
        public ParserException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
