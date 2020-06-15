//  Contains convenient assertions to enhance code readability.
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

namespace VoiceCoder.Util
{
    /// <summary>
    /// Designed for convenience one line checks to clear up code when writing
    /// various assertions. This was based off of the idea from Google's Guava
    /// library for Java.
    /// </summary>
    public static class Assertion
    {
        /// <summary>
        /// Checks that the object is not null. Throws ArgumentNullException
        /// if the argument is null.
        /// </summary>
        /// <param name="obj">The object to check for being null.</param>
        /// <exception cref="ArgumentNullException">Thrown if the object is
        /// null.</exception>
        public static void CheckNotNull(object obj)
        {
            CheckNotNull(obj, null);
        }

        /// <summary>
        /// Checks that the object is not null. Throws ArgumentNullException
        /// if the argument is null. If the reason is not null, then the 
        /// reason is passed to the exception's message value.
        /// </summary>
        /// <param name="obj">The object to check for being null.</param>
        /// <param name="reason">The reason for this exception.</param>
        /// <exception cref="ArgumentNullException">Thrown if the object is
        /// null.</exception>
        public static void CheckNotNull(object obj, string reason)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("", reason);
            }
        }

        /// <summary>
        /// Checks that the statement is true. Throws ArgumentException if the
        /// argument is null.
        /// </summary>
        /// <param name="obj">The statement that is expected to hold true.
        /// </param>
        /// <exception cref="ArgumentException">Thrown if the statement has
        /// evaluated to false.</exception>
        public static void CheckArgument(bool statement)
        {
            CheckArgument(statement, null);
        }

        /// <summary>
        /// Checks that the statement is true. Throws ArgumentException if the
        /// argument is null. Allows for a reason to be provided, which may be
        /// null.
        /// </summary>
        /// <param name="obj">The statement that is expected to hold true.
        /// </param>
        /// <param name="reason">The reason for this exception.</param>
        /// <exception cref="ArgumentException">Thrown if the statement has
        /// evaluated to false.</exception>
        public static void CheckArgument(bool statement, string reason)
        {
            if (!statement)
            {
                throw new ArgumentException(reason);
            }
        }
    }
}
