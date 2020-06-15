//  Tokenizes the input provided so they can be processed by a compiler.
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

using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static System.Diagnostics.Debug;
using static VoiceCoder.Util.Assertion;

namespace VoiceCoder.Parser
{
    /// <summary>
    /// Takes a line of text and turns it into a list of tokens.
    /// </summary>
    public class Tokenizer
    {
        /// <summary>
        /// A precompiled matcher for an identifier.
        /// </summary>
        private static readonly Regex IDENTIFIER_REGEX = new Regex(@"^([A-Za-z_]+\.)*[A-Za-z_]+$");

        /// <summary>
        /// The symbols allowed after a number. Required so that in the future
        /// if there is ever support for non-ASCII characters, it could go
        /// beyond the standard set.
        /// </summary>
        private static readonly char[] VALID_SYMBOLS_AFTER_NUMBER = new char[] {
            ' ', '\n', '\r', '\t', '(', ')', '[', ']', '{', '}', '<', '>', '=', ';', '|', ','
        };

        /// <summary>
        /// A delegate for seeing if a character is valid and should be part
        /// of the final token.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if it is a valid character, false otherwise.
        /// </returns>
        private delegate bool ValidCharDelegate(char c);

        /// <summary>
        /// A delegate for seeing if a character is illegal and should throw a
        /// tokenizer exception.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if it is a valid character, false otherwise.
        /// </returns>
        private delegate bool IllegalCharDelegate(char c);

        /// <summary>
        /// The list of lexed tokens.
        /// </summary>
        private List<Token> tokens;

        /// <summary>
        /// The line number being tracked when lexing.
        /// </summary>
        private int lineNumber;

        /// <summary>
        /// The offset of the character.
        /// </summary>
        private int charOffset;

        /// <summary>
        /// Lexes the text provided into all the tokens.
        /// </summary>
        /// <param name="text">The lines of text as one string.</param>
        /// <exception cref="TokenizerException">If there is any error when
        /// tokenizing the data.</exception>
        public Tokenizer(string text)
        {
            lineNumber = 1;
            tokens = new List<Token>();
            GenerateTokens(text);
        }

        /// <summary>
        /// Reads all the text in from the file at the provided path and will
        /// tokenize all the input.
        /// </summary>
        /// <param name="path">The path to the file to tokenize.</param>
        /// <returns>A tokenizer for the file path provided.</returns>
        /// <exception cref="TokenizerException">If there is an error with
        /// tokenizing the data from the file.</exception>
        /// <exception cref="ArgumentException">If the path length is invalid.
        /// </exception>
        /// <exception cref="ArgumentNullException">If the path is null.
        /// </exception>
        /// <exception cref="PathTooLongException">If the path name is too 
        /// long.</exception>
        /// <exception cref="DirectoryNotFoundException">If the path directory
        /// could not be found.</exception>
        /// <exception cref="IOException">If any IO error occurs.</exception>
        /// <exception cref="UnauthorizedAccessException">If there is not
        /// permission from the user, or the file is read only, or the reading
        /// is not allowed on the platform, or if it's a directory.</exception>
        /// <exception cref="FileNotFoundException">If the file could not be
        /// found. This is also thrown if it's a directory.</exception>
        /// <exception cref="NotSupportedException">If the path is an invalid
        /// format.</exception>
        /// <exception cref="SecurityException">If the caller does not have 
        /// the required permission.</exception>
        public static Tokenizer FromFile(string path)
        {
            CheckNotNull(path);
            CheckArgument(path.Length > 0);

            string text = File.ReadAllText(path);
            return new Tokenizer(text);
        }

        /// <summary>
        /// Takes the text to be processed and lexes them into tokens.
        /// </summary>
        /// <param name="text">The text to process.</param>
        /// <exception cref="TokenizerException">If there is an error with
        /// tokenizing the data from the file.</exception>
        private void GenerateTokens(string text)
        {
            Assert(text != null);

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // If performance becomes an issue ever, this could be turned to a switch.
                // This also prevents the usage of a ton of breaks/continues.
                // Notes:
                // - \r is ignored and treated as whitespace, only \n is identified as a new line
                // - If passed onto a function, it will take care of line number/char offsets
                if (char.IsLetter(c))
                {
                    ConsumeWord(text, ref i);
                }
                else if (c == ' ' || c == '\t' || c == '\r')
                {
                    charOffset++;
                }
                else if (c == '\n')
                {
                    charOffset = 0;
                    lineNumber++;
                }
                else if (c == '$' || c == '@')
                {
                    ExtractIdentifier(text, ref i, c);
                }
                else if (c == '#')
                {
                    ConsumeComment(text, ref i);
                }
                else if (c == '"')
                {
                    ConsumeQuotedString(text, ref i);
                }
                else if (char.IsDigit(c))
                {
                    ConsumeNumber(text, ref i);
                }
                else
                {
                    TokenType tokenType = TokenTypeFromSymbol(c);
                    Token token = new Token(tokenType, char.ToString(c), lineNumber, charOffset);
                    tokens.Add(token);
                    charOffset++; // We always consumed at least one character (since this isn't a new line).
                }
            }
        }

        /// <summary>
        /// Returns the symbol for this token type (or errors out).
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>The token type for this character.</returns>
        /// <exception cref="TokenizerException">If this is not a symbol for a
        /// token type.</exception>
        private TokenType TokenTypeFromSymbol(char c)
        {
            TokenType tokenType = TokenType.NONE;
            switch (c)
            {
                case '(':
                    tokenType = TokenType.ParenStart;
                    break;
                case ')':
                    tokenType = TokenType.ParenEnd;
                    break;
                case '[':
                    tokenType = TokenType.BracketStart;
                    break;
                case ']':
                    tokenType = TokenType.BracketEnd;
                    break;
                case '<':
                    tokenType = TokenType.AngleStart;
                    break;
                case '>':
                    tokenType = TokenType.AngleEnd;
                    break;
                case '{':
                    tokenType = TokenType.CurlyStart;
                    break;
                case '}':
                    tokenType = TokenType.CurlyEnd;
                    break;
                case '=':
                    tokenType = TokenType.Equals;
                    break;
                case ';':
                    tokenType = TokenType.Semicolon;
                    break;
                case '|':
                    tokenType = TokenType.Pipe;
                    break;
                case '.':
                    tokenType = TokenType.Period;
                    break;
                case '*':
                    tokenType = TokenType.Star;
                    break;
                case '+':
                    tokenType = TokenType.Plus;
                    break;
                case ',':
                    tokenType = TokenType.Comma;
                    break;
                default:
                    throw new TokenizerException(lineNumber, charOffset, "Unexpected character: " + c);
            }

            Assert(tokenType != TokenType.NONE);
            return tokenType;
        }

        /// <summary>
        /// Extracts the token based on the provided arguments. Increments the
        /// loop counter but not the charOffset/lineNumber. Same as calling
        /// ExtractElementToToken except with a null IsIllegalDelegate.
        /// </summary>
        /// <param name="text">The character stream to tokenize (in string
        /// format).</param>
        /// <param name="i">The character offset (will be modified).</param>
        /// <param name="ValidCharDel">A delegate to determine what is a valid
        /// character and thus part of the token.True means the character is
        /// accepted, false means it is not.</param>
        /// <returns>The extracted token.</returns>
        private string ExtractElementToToken(string text, ref int i, ValidCharDelegate ValidCharDel)
        {
            return ExtractElementToToken(text, ref i, ValidCharDel, null);
        }

        /// <summary>
        /// Extracts the token based on the provided arguments. Increments the
        /// loop counter but not the charOffset/lineNumber.
        /// </summary>
        /// <param name="text">The character stream to tokenize (in string
        /// format).</param>
        /// <param name="i">The character offset (will be modified).</param>
        /// <param name="ValidCharDel">A delegate to determine what is a valid
        /// character and thus part of the token.True means the character is
        /// accepted, false means it is not.</param>
        /// <param name="IllegalCharDel">A delegate, which may be null, whereby
        /// not being null will cause an exception to be thrown if it returns
        /// true.</param>
        /// <returns>The extracted token.</returns>
        /// <exception cref="TokenizerException">If IllegalCharDel is not null
        /// and it detects an illegal character.</exception>
        private string ExtractElementToToken(string text, ref int i, ValidCharDelegate ValidCharDel, IllegalCharDelegate IllegalCharDel)
        {
            Assert(text != null);
            Assert(i < text.Length);
            Assert(ValidCharDel != null);

            char c;
            bool isValidChar = false;
            int tempCharOffset = charOffset;
            StringBuilder stringBuilder = new StringBuilder();
            do
            {
                c = text[i];

                if (IllegalCharDel != null && IllegalCharDel(c))
                {
                    throw new TokenizerException(lineNumber, tempCharOffset, $"Unexpected character: {c.ToString()}");
                }

                isValidChar = ValidCharDel(c);
                if (isValidChar)
                {
                    stringBuilder.Append(c);
                    i++; // Only advance if it's a valid character.
                }

                tempCharOffset++;
            } while (i < text.Length && isValidChar);

            // Since the GenerateToken method for-loop will increment for us, we need to rewind prematurely.
            // When we rewind, we make it so that when the for-loop does increment i, it will then look at
            // the character that caused the above to terminate and assign that (or skip) as needed.
            i--;

            Assert(stringBuilder.Length > 0);
            string wordStr = stringBuilder.ToString();
            return wordStr;
        }

        /// <summary>
        /// Consumes a word from the file character stream. Advances the stream
        /// index after consuming some word.
        /// </summary>
        /// <param name="text">All the characters the tokenizer is tokenizing.
        /// </param>
        /// <param name="i">The offset that this should consume the word from.
        /// </param>
        /// <exception cref="TokenizerException">If there are bad characters
        /// in the word (like ab$d or he0p or some@) </exception>
        private void ConsumeWord(string text, ref int i)
        {
            Assert(text != null);
            Assert(i < text.Length);

            ValidCharDelegate del = c => char.IsLetter(c);
            IllegalCharDelegate illegalDel = c => char.IsDigit(c) || c == '$' || c == '@';
            string tokenStr = ExtractElementToToken(text, ref i, del, illegalDel);

            tokens.Add(new Token(TokenType.Word, tokenStr, lineNumber, charOffset));
            charOffset += tokenStr.Length;
        }

        /// <summary>
        /// Will consume a quoted string with any character in the quotes
        /// (except escape sequences, tab is allowed).
        /// </summary>
        /// <param name="text">The character stream to be tokenized.</param>
        /// <param name="i">The current index offset.</param>
        /// <exception cref="TokenizerException">If the quotation mark ending
        /// is missing or the end is reached before finding it.</exception>
        private void ConsumeQuotedString(string text, ref int i)
        {
            Assert(text != null);
            Assert(i < text.Length);

            // Because we will be skipping the first quotation mark, we want to make sure
            // 'i' is actually still valid. Otherwise if there is no next character than we
            // know it's a malformed quote.
            i++;
            if (i >= text.Length)
            {
                throw new TokenizerException(lineNumber, charOffset, "Found starting quote at EOF.");
            }

            ValidCharDelegate del = c => c != 127 && (c >= 32 || c == '\t') && c != '"';
            string tokenStr = ExtractElementToToken(text, ref i, del);

            // Now we actually want to skip past the last quotation mark since we didn't consume it.
            // We need to make some logic checks with this since fringe cases could yield EOF issues.
            i++;
            if (tokenStr.Length <= 0)
            {
                throw new TokenizerException(lineNumber, charOffset, "Cannot have an empty quoted string.");
            }
            else if (i >= text.Length)
            {
                throw new TokenizerException(lineNumber, charOffset, "Quotation mark not found (EOF).");
            }
            else if (text[i] != '"')
            {
                throw new TokenizerException(lineNumber, charOffset, $"Could not find ending quotation mark, got '{text[i]}' instead.");
            }

            tokens.Add(new Token(TokenType.QuotedString, tokenStr, lineNumber, charOffset));
            charOffset += tokenStr.Length + 2; // +2 for two quotation marks.
        }

        /// <summary>
        /// Reads in a number (only integers, not floats).
        /// </summary>
        /// <param name="text">The character stream to be tokenized.</param>
        /// <param name="i">The current index offset.</param>
        /// <exception cref="TokenizerException">If the number is invalid,
        /// such as multiple decimal points or characters.</exception>
        private void ConsumeNumber(string text, ref int i)
        {
            Assert(text != null);
            Assert(i < text.Length);

            ValidCharDelegate validDel = c => char.IsDigit(c);
            IllegalCharDelegate illegalDel = c => !(char.IsDigit(c) || VALID_SYMBOLS_AFTER_NUMBER.Contains(c));
            string tokenStr = ExtractElementToToken(text, ref i, validDel, illegalDel);

            // This function does not support floating point numbers. May be changed in the future.
            if (tokenStr.Contains('.'))
            {
                throw new TokenizerException(lineNumber, charOffset, "Floating point numbers not supported.");
            }

            int dummy = 0; // TryParse needs an 'out' variable, we don't use it though.
            if (!int.TryParse(tokenStr, out dummy))
            {
                throw new TokenizerException(lineNumber, charOffset, $"Malformed number: {tokenStr}");
            }

            tokens.Add(new Token(TokenType.Number, tokenStr, lineNumber, charOffset));
            charOffset += tokenStr.Length;
        }

        /// <summary>
        /// Causes the index to go past the comment line.
        /// </summary>
        /// <param name="text">The character stream to be tokenized.</param>
        /// <param name="i">The current index offset.</param>
        private void ConsumeComment(string text, ref int i)
        {
            Assert(text != null);
            Assert(i < text.Length);

            ExtractElementToToken(text, ref i, c => c != '\n');
            // FUTURE NOTE: This only works since we don't care about the result and that it is
            // the end of the line. Therefore charOffset doesn't matter since it's ignored.
            // In the future, if the comments are to be extracted, there *must* be some incrementing
            // done here for charOffset.
        }

        /// <summary>
        /// Gets an identifier and then assigns the token type based on the
        /// symbol prefix.
        /// </summary>
        /// <param name="text">The character stream to be tokenized.</param>
        /// <param name="i">The current index offset.</param>
        /// <param name="symbolPrefix">The symbol that prefixes this 
        /// identifier, used in determining what kind of token it is.</param>
        /// <exception cref="TokenizerException">If the identifier is 
        /// malformed.</exception>
        private void ExtractIdentifier(string text, ref int i, char symbolPrefix)
        {
            Assert(text != null);
            Assert(i < text.Length);
            Assert(symbolPrefix == '$' || symbolPrefix == '@');

            i++; // Jump past the symbol, we don't need it anymore.

            ValidCharDelegate validDel = c => char.IsLetter(c) || c == '_' || c == '.';
            string tokenStr = ExtractElementToToken(text, ref i, validDel);

            if (!IDENTIFIER_REGEX.Match(tokenStr).Success)
            {
                throw new TokenizerException(lineNumber, charOffset, $"Malformed identifier: {tokenStr}");
            }

            TokenType type = symbolPrefix == '$' ? TokenType.DollarIdentifier : TokenType.AtIdentifier;
            tokens.Add(new Token(type, tokenStr, lineNumber, charOffset));
            charOffset += 1 + tokenStr.Length; // 1 char symbol + token length characters read.
        }

        /// <summary>
        /// Creates a deep copy of the tokens and returns a new list.
        /// </summary>
        /// <returns>A new list of tokens.</returns>
        public List<Token> GetTokens()
        {
            List<Token> newList = new List<Token>();
            tokens.ForEach(t => newList.Add(new Token(t)));
            return newList;
        }

        /// <summary>
        /// Gets a TokenIterator for the tokens generated by this Tokenizer.
        /// This is a deep copy of all the tokens.
        /// </summary>
        /// <returns>The TokenIterator for the generated tokens.</returns>
        public TokenIterator GetTokenIterator()
        {
            return new TokenIterator(GetTokens());
        }
    }
}
