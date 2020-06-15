//  A class containing token information for tokenized grammar files.
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

using static VoiceCoder.Util.Assertion;

namespace VoiceCoder.Parser
{
    /// <summary>
    /// Represents all the different token types that can be parsed.
    /// </summary>
    public enum TokenType
    {
        NONE = 0,           // 0 - Reserved/unused
        Number = 1,         // 1
        Float,              // 2
        Word,               // 3
        DollarIdentifier,   // 4
        AtIdentifier,       // 5
        QuotedString,       // 6
        ParenStart,         // 7
        ParenEnd,           // 8
        BracketStart,       // 9
        BracketEnd,         // 10
        CurlyStart,         // 11
        CurlyEnd,           // 12
        AngleStart,         // 13
        AngleEnd,           // 14
        Equals,             // 15
        Semicolon,          // 16
        Pipe,               // 17
        Period,             // 18
        Star,               // 19
        Plus,               // 20
        Comma               // 21
    };

    /// <summary>
    /// Encapsulates a smallest element from a parsed grammar file or stream
    /// of characters.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// The type of token this is.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// The text that makes up this token.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The line number this token was found at.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// The character offset from the line number provided.
        /// </summary>
        public int CharOffset { get; }

        /// <summary>
        /// The character offset from the beginning of the line for which
        /// this token was found at.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="text"></param>
        /// <param name="lineNumber"></param>
        /// <param name="charOffset"></param>
        /// <exception cref="ArgumentNullException">If the text is null.
        /// </exception>
        /// <exception cref="ArgumentException">If the type is the NONE token
        /// type, or the line number or character offset are negative, or if
        /// the string is empty.</exception>
        public Token(TokenType type, string text, int lineNumber, int charOffset)
        {
            CheckNotNull(text);
            CheckArgument(text.Length > 0);
            CheckArgument(lineNumber >= 0);
            CheckArgument(charOffset >= 0);
            CheckArgument(type != TokenType.NONE);

            Type = type;
            Text = text;
            LineNumber = lineNumber;
            CharOffset = charOffset;
        }

        /// <summary>
        /// A token copy constructor.
        /// </summary>
        /// <param name="token">The token to copy.</param>
        /// <exception cref="ArgumentNullException">If the token is null.
        /// </exception>
        public Token(Token token)
        {
            CheckNotNull(token);

            Type = token.Type;
            Text = token.Text;
            LineNumber = token.LineNumber;
            CharOffset = token.CharOffset;
        }

        /// <summary>
        /// Gets the hashcode for this token.
        /// </summary>
        /// <returns>The hashcode for this token.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Checks to see if the tokens are equal.
        /// </summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>True if they're equal, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            // Check if this is itself, if so quickly exit. Also handles nulls.
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            Token token = obj as Token;

            if (token == null)
            {
                return false;
            }

            return token.Type == Type &&
                   token.Text == Text &&
                   token.LineNumber == LineNumber &&
                   token.CharOffset == CharOffset;
        }

        /// <summary>
        /// Converts this to a readable string format.
        /// </summary>
        /// <returns>A string representation.</returns>
        public override string ToString()
        {
            return $"<Token[Type={Type},Text={Text},LineNumber={LineNumber},CharOffset={CharOffset}]>";
        }
    }
}
