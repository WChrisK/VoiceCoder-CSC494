//  Representation of a file and compiled elements in a Compiler.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static System.Diagnostics.Debug;
using static VoiceCoder.Util.Assertion;

namespace VoiceCoder.Parser
{
    /// <summary>
    /// Encapsulates a specific file compilation.
    /// </summary>
    public class Module
    {
        /// <summary>
        /// The package path, which is based on how it was parsed from the
        /// root directory of where the user wanted to import from.
        /// </summary>
        public string DirectoryPackagePath { get; private set; }

        /// <summary>
        /// The name of the file, does not contain the extension.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// The python file path for this module (if available).
        /// </summary>
        public string PythonFilePath { get; private set; }

        /// <summary>
        /// The full path to the file on the hard disk.
        /// </summary>
        private string fullPath;

        /// <summary>
        /// The token iterator that has the tokens to be compiled from.
        /// </summary>
        private TokenIterator tokenIterator;

        #region Import Fields - Fields used in package processing.

        /// <summary>
        /// A lookup table (and a container) of all the imports, which are
        /// mapped to their rename if any (if not, empty string) and a boolean
        /// value if the import is static (true) or not (false). This is read
        /// only.
        /// </summary>
        public ReadOnlyDictionary<string, Tuple<string, bool>> LoadedImports
        {
            get { return new ReadOnlyDictionary<string, Tuple<string, bool>>(loadedImports); }
        }

        /// <summary>
        /// The actual backing data structure for the loaded imports.
        /// A lookup table (and a container) of all the imports, which are
        /// mapped to their rename if any (if not, empty string) and a boolean
        /// value if the import is static (true) or not (false).
        /// </summary>
        private Dictionary<string, Tuple<string, bool>> loadedImports;

        /// <summary>
        /// If the import is static or not.
        /// </summary>
        private bool importIsStatic;

        /// <summary>
        /// The name of the package. Should not be null.
        /// </summary>
        private string packageName = "";

        /// <summary>
        /// The package identifier. Should not be null.
        /// </summary>
        private string packageRenameIdentifier = "";

        #endregion

        #region Rule fields - Fields used in compiling rules.

        /// <summary>
        /// The list builder which allows us to make all the connections and
        /// quickly retrieve a compiled list after all the tokens have been
        /// analyzed.
        /// </summary>
        private HierarchialListBuilder currentListBuilder;

        /// <summary>
        /// The name of the function for this rule.
        /// </summary>
        private string ruleFunctionName;

        /// <summary>
        /// The last read in repeat minimum range.
        /// </summary>
        private int lastMinRange;

        /// <summary>
        /// The last read in repeat maximum range.
        /// </summary>
        private int lastMaxRange;

        /// <summary>
        /// A collection of the hierarchial linked list set of nodes that
        /// define a grammar rule, organized by variable name (key).
        /// </summary>
        public ReadOnlyDictionary<string, HierarchicalNode> LoadedGrammarNodes
        {
            get { return new ReadOnlyDictionary<string, HierarchicalNode>(loadedGrammarNodes); }
        }

        /// <summary>
        /// The actual backing data structure for the loaded rules.
        /// A collection of the hierarchial linked list set of nodes that
        /// define a grammar rule, organized by variable name (key).
        /// </summary>
        private Dictionary<string, HierarchicalNode> loadedGrammarNodes;

        #endregion

        /// <summary>
        /// A function to call that will attempt consumption and possibly not
        /// succeed.
        /// </summary>
        /// <returns>True on success, false if it failed.</returns>
        private delegate bool ConsumeDelegate();

        /// <summary>
        /// Creates a module from the package path, name, and the file path on
        /// the hard disk.
        /// </summary>
        /// <param name="packagePath">The package path in dotted form (ex:
        /// my.package.path).</param>
        /// <param name="fileName">The name of the file without an extension.
        /// </param>
        /// <param name="entirePath">The full path to the file on the hard 
        /// disk.</param>
        /// <exception cref="ArgumentNullException">If any argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">If the file name or the path
        /// are empty strings.</exception>
        public Module(string packagePath, string fileName, string entirePath)
        {
            CheckNotNull(packagePath);
            CheckNotNull(fileName);
            CheckNotNull(entirePath);
            CheckArgument(fileName.Length > 0);
            CheckArgument(entirePath.Length > 0);

            DirectoryPackagePath = packagePath;
            FileName = fileName;
            fullPath = entirePath;
            PythonFilePath = "";
            loadedImports = new Dictionary<string, Tuple<string, bool>>();
            loadedGrammarNodes = new Dictionary<string, HierarchicalNode>();
        }

        public void SetPythonFile(string pythonPath)
        {
            CheckNotNull(pythonPath);
            CheckArgument(pythonPath.Length > 0);
        }

        #region Consumption Functions - General consumption functions with delegates.

        /// <summary>
        /// Takes a list of functions that should be called and runs them
        /// until the first one succeeds (by that function returning true).
        /// The token iterator will not advance unless there is a successful
        /// consumer function call.
        /// </summary>
        /// <param name="consumerFunctions"></param>
        /// <returns>True on success, false on failure.</returns>
        private bool ConsumeAny(params ConsumeDelegate[] consumerFunctions)
        {
            Assert(consumerFunctions != null);
            Assert(consumerFunctions.Length != 0);

            foreach (ConsumeDelegate consumerFunc in consumerFunctions)
            {
                int marker = tokenIterator.GetMarker();

                // Call the definition we'd like to attempt to consume.
                // If we match a grammar definition, the delegate will return true.
                if (consumerFunc())
                {
                    return true;
                }

                // Rewind if we fail since the definition was not found.
                tokenIterator.SetToMarker(marker);
            }

            return false;
        }

        /// <summary>
        /// Consumes all the functions provided sequentially. If any throw or
        /// fail to complete, the token iterator will be rolled back before
        /// starting and return false. Otherwise it will run all the functions
        /// and return true.
        /// </summary>
        /// <param name="consumerFunctions">The functions to call, should not
        /// be null or empty.</param>
        /// <returns>True on success, false if one of the functions returned
        /// false.</returns>
        private bool ConsumeAllSequentially(params ConsumeDelegate[] consumerFunctions)
        {
            Assert(consumerFunctions != null);
            Assert(consumerFunctions.Length != 0);

            int marker = tokenIterator.GetMarker();

            foreach (ConsumeDelegate consumerFunc in consumerFunctions)
            {
                // Roll back if any fail.
                if (!consumerFunc())
                {
                    tokenIterator.SetToMarker(marker);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Keeps consuming (calling) the function for token consumption until
        /// it fails. Will advance the stream always until the last point of
        /// failure. Similar to the Kleene Star.
        /// </summary>
        /// <param name="consumerFunc">The function to call.</param>
        /// <returns>True always (this cannot fail).</returns>
        private bool ConsumeZeroOrMore(ConsumeDelegate consumerFunc)
        {
            Assert(consumerFunc != null);

            bool consumed = true;
            while (consumed)
            {
                int marker = tokenIterator.GetMarker();
                consumed = consumerFunc();

                if (!consumed)
                {
                    tokenIterator.SetToMarker(marker);
                }
            }

            return true;
        }

        /// <summary>
        /// Keeps consuming (calling) the function for token consumption until
        /// it fails. It will only return true though if at least one of the
        /// function calls succeed. Upon failure, it will only rewind back to
        /// the point of failure. This is greedy.
        /// </summary>
        /// <param name="consumerFunc">The function to call.</param>
        /// <returns>True if at least one was consumed, false if none was
        /// consumed.</returns>
        private bool ConsumeOneOrMore(ConsumeDelegate consumerFunc)
        {
            Assert(consumerFunc != null);

            int marker = tokenIterator.GetMarker();

            // Needs to consume at least one.
            if (!consumerFunc())
            {
                tokenIterator.SetToMarker(marker);
                return false;
            }

            // Now do a greedy consumption.
            bool consumed = true;
            while (consumed)
            {
                marker = tokenIterator.GetMarker();
                consumed = consumerFunc();

                if (!consumed)
                {
                    tokenIterator.SetToMarker(marker);
                }
            }

            return true;
        }

        /// <summary>
        /// Takes a list of functions to call, and tries them all. 
        /// If one of them passes, this will always return true. Can 
        /// be viewed as a combo of running ConsumeOneOrMore(ConsumeAny(...)).
        /// Note that this is not intended to be used where multiple branching
        /// paths are possible, as this will exit. This should not be treated
        /// like a finite state machine since it will not go down every 
        /// possible path, but rather choose the first working one and advance.
        /// </summary>
        /// <param name="consumerFunctions">The functions to call.</param>
        /// <returns>True on success if one function passed once, false on 
        /// failure.</returns>
        private bool ConsumeOneOrMoreAny(params ConsumeDelegate[] consumerFunctions)
        {
            Assert(consumerFunctions != null);
            Assert(consumerFunctions.Length != 0);

            // Check to see that at least one function passes, or else we have
            // to rewind and return false.
            bool oneFunctionPassed = false;
            int startMarker = tokenIterator.GetMarker();

            foreach (ConsumeDelegate consumerFunc in consumerFunctions)
            {
                int marker = tokenIterator.GetMarker();
                if (consumerFunc())
                {
                    oneFunctionPassed = true;
                    break;
                }
                tokenIterator.SetToMarker(marker);
            }

            // If nothing passed, this fails since we require at least one.
            if (!oneFunctionPassed)
            {
                tokenIterator.SetToMarker(startMarker);
                return false;
            }

            // Now we can consume greedily until we run out of matches.
            bool moreMatches = true;
            do
            {
                moreMatches = false;
                foreach (ConsumeDelegate consumerFunc in consumerFunctions)
                {
                    int marker = tokenIterator.GetMarker();
                    if (consumerFunc())
                    {
                        moreMatches = true;
                        break;
                    }
                    tokenIterator.SetToMarker(marker);
                }
            } while (moreMatches);

            return true;
        }

        /// <summary>
        /// Advances the tokenizer to the end of the next semicolon. If EOF is
        /// found, it will be pointing past the end of the TokenIterator.
        /// </summary>
        private void AdvancePastNextSemicolonIfAny()
        {
            // Keep consuming until EOF (or we break out if we pass a semicolon).
            bool foundSemicolon = false;
            while (tokenIterator.HasNext() && foundSemicolon)
            {
                Token token = tokenIterator.Next();
                foundSemicolon = (token.Type == TokenType.Semicolon);
            }
        }

        /// <summary>
        /// Consumes the token type if present. Only advances the stream if
        /// the token type exists.
        /// </summary>
        /// <returns>True if consumed, false otherwise.</returns>
        private bool ConsumeTokenTypeIfPresent(TokenType tokenType)
        {
            Assert(tokenType != TokenType.NONE);

            if (!tokenIterator.HasNextType(tokenType))
            {
                return false;
            }

            tokenIterator.Next();
            return true;
        }

        /// <summary>
        /// Consumes this symbol, only advances the stream if a match.
        /// </summary>
        /// <returns>True if consumed, false otherwise.</returns>
        private bool ConsumeSemicolon()
        {
            return ConsumeTokenTypeIfPresent(TokenType.Semicolon);
        }

        /// <summary>
        /// Consumes this symbol, only advances the stream if a match.
        /// </summary>
        /// <returns>True if consumed, false otherwise.</returns>
        private bool ConsumeEquals()
        {
            return ConsumeTokenTypeIfPresent(TokenType.Equals);
        }

        /// <summary>
        /// Consumes this symbol, only advances the stream if a match.
        /// </summary>
        /// <returns>True if consumed, false otherwise.</returns>
        private bool ConsumeLeftCurlyBrace()
        {
            return ConsumeTokenTypeIfPresent(TokenType.CurlyStart);
        }

        /// <summary>
        /// Consumes this symbol, only advances the stream if a match.
        /// </summary>
        /// <returns>True if consumed, false otherwise.</returns>
        public bool ConsumeRightCurlyBrace()
        {
            return ConsumeTokenTypeIfPresent(TokenType.CurlyEnd);
        }

        #endregion

        #region Tokenizing - All the tokenizing method

        /// <summary>
        /// Reads all the text in from the file at the provided path for this
        /// constructor.
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
        public void Tokenize()
        {
            tokenIterator = Tokenizer.FromFile(fullPath).GetTokenIterator();
        }

        #endregion

        #region Import - All the methods for consumption of imports.

        /// <summary>
        /// Consumes all the imports in this module. Errors out if the tokens
        /// are not valid (meaning there should be either import or rule
        /// statements and nothing else).
        /// </summary>
        /// <exception cref="CompilerException">If an import is malformed.
        /// </exception>
        public void DoImportAndTokenValidityPass()
        {
            tokenIterator.ResetToBeginning();

            while (tokenIterator.HasNext())
            {
                Token token = tokenIterator.Next();

                // If we land on an rule instead of an import, consume until the semicolon.
                if (token.Type == TokenType.DollarIdentifier)
                {
                    AdvancePastNextSemicolonIfAny();
                }
                else if (token.Type == TokenType.Word && token.Text.ToLower().Equals("import"))
                {
                    // If we do land on an import, consume or error out if it fails.
                    if (!ConsumeImport())
                    {
                        throw new CompilerException($"{DirectoryPackagePath}.{FileName} Malformed input statement on line {token.LineNumber}.");
                    }
                }
            }
        }

        /// <summary>
        /// Consumes an import. Assumes the first token ("import" word) has
        /// been read already.
        /// </summary>
        /// <returns>True on success, false on failure.</returns>
        /// <exception cref="CompilerException">If an import is malformed, or
        /// if the import already has been loaded.</exception>
        private bool ConsumeImport()
        {
            importIsStatic = false;
            packageName = "";
            packageRenameIdentifier = "";

            // This will rollback for us on failure, so it's okay to not worry
            // about marking the position here.
            bool validImport = ConsumeAllSequentially(ConsumeOptionalImportStatic,
                                                      ConsumePackage,
                                                      ConsumeOptionalPackageIdentifier,
                                                      ConsumeSemicolon);
            if (validImport)
            {
                Assert(packageName != null);
                Assert(packageRenameIdentifier != null);
                Assert(packageName.Length > 0);

                if (LoadedImports.ContainsKey(packageName))
                {
                    throw new CompilerException($"Package {packageName} already loaded.");
                }

                // A static package cannot be renamed because static means the
                // qualifier name is not considered (since it's in the global
                // namespace), so both together is not allowed.
                if (packageRenameIdentifier.Length > 0 && importIsStatic)
                {
                    throw new CompilerException($"Package {packageName} cannot be both static and renamed.");
                }

                loadedImports[packageName] = Tuple.Create(packageRenameIdentifier, importIsStatic);
            }

            return validImport;
        }

        /// <summary>
        /// Consumes an optional static import statement if it exists. Will
        /// advance the stream on success or leave it untouched on failure.
        /// </summary>
        /// <returns>True always, regardless of consumption success or 
        /// failure.</returns>
        private bool ConsumeOptionalImportStatic()
        {
            if (tokenIterator.HasNextType(TokenType.Word))
            {
                int marker = tokenIterator.GetMarker();
                Token token = tokenIterator.Next();

                if (token.Text.ToLower().Equals("static"))
                {
                    importIsStatic = true;
                }
                else
                {
                    tokenIterator.SetToMarker(marker);
                }
            }

            return true;
        }

        /// <summary>
        /// Consumes the package import.
        /// </summary>
        /// <returns>True if consumed, false if stream end or invalid tokens.
        /// </returns>
        private bool ConsumePackage()
        {
            if (!tokenIterator.HasNextType(TokenType.Word))
            {
                return false;
            }

            Token token = tokenIterator.Next();
            packageName = token.Text;

            // We will have gotten the package base before any periods, this will
            // capture all the periods and package subdirectories as needed (if any).
            ConsumeZeroOrMore(ConsumeAdditionalPackageQualifiers);

            return true;
        }

        /// <summary>
        /// Consumes a period and word and pushes data to the package name.
        /// This advances the stream on failure, so it must be reset if this
        /// returns false manually.
        /// </summary>
        /// <returns>True if it consumed a period and word, false otherwise.
        /// </returns>
        private bool ConsumeAdditionalPackageQualifiers()
        {
            if (!tokenIterator.HasNextType(TokenType.Period))
            {
                return false;
            }

            tokenIterator.Next();
            packageName += ".";

            if (!tokenIterator.HasNextType(TokenType.Word))
            {
                return false;
            }

            packageName += tokenIterator.Next().Text;

            return true;
        }

        /// <summary>
        /// Consumes an optional package rename statement. Rolls back if
        /// failure occurs. Sets the rename attribute to a non-empty string.
        /// </summary>
        /// <returns>True always.</returns>
        private bool ConsumeOptionalPackageIdentifier()
        {
            if (!tokenIterator.HasNextType(TokenType.Word))
            {
                return true;
            }

            int marker = tokenIterator.GetMarker();
            Token token = tokenIterator.Next();

            if (!token.Text.ToLower().Equals("as") || !tokenIterator.HasNextType(TokenType.Word))
            {
                tokenIterator.SetToMarker(marker);
                return true;
            }

            packageRenameIdentifier = tokenIterator.Next().Text;

            return true;
        }

        #endregion

        #region Rules - All the methods for consumption of rules.

        /// <summary>
        /// Passes over and consumes the rules.
        /// </summary>
        public void DoRulePass()
        {
            tokenIterator.ResetToBeginning();

            while (tokenIterator.HasNext())
            {
                Token token = tokenIterator.Next();

                // If we land on an rule instead of an import, consume until 
                // the semicolon. Note that it is safe to call the 'advance to
                // next semicolon' since we made sure by the import pass that 
                // all the tokens are valid. Thus, any non-dollar identifiers
                // must be an import and we can safely pass by.
                if (token.Type == TokenType.DollarIdentifier)
                {
                    if (!ConsumeRule(token))
                    {
                        throw new CompilerException($"{DirectoryPackagePath}.{FileName} Bad definition on line {token.LineNumber}.");
                    }
                }
                else
                {
                    AdvancePastNextSemicolonIfAny();
                }
            }
        }

        /// <summary>
        /// Consumes a single rule. Rolls back on failure.
        /// </summary>
        /// <param name="initialToken">The token for the head of the list.
        /// </param>
        /// <returns>True if a rule was consumed, false if there was an error
        /// (meaning corrupt rule).</returns>
        private bool ConsumeRule(Token initialToken)
        {
            Assert(initialToken != null);
            Assert(initialToken.Type == TokenType.DollarIdentifier);

            currentListBuilder = new HierarchialListBuilder(initialToken.Text);
            ruleFunctionName = "";

            bool validRule = ConsumeAllSequentially(ConsumeOptionalRuleFunction,
                                                    ConsumeEquals,
                                                    ConsumeExpression,
                                                    ConsumeSemicolon);
            if (validRule)
            {
                loadedGrammarNodes[initialToken.Text] = currentListBuilder.GetRootNode();
            }

            return validRule;
        }

        /// <summary>
        /// Consumes an optional rule that may exist. If so, advances the
        /// stream, otherwise the stream isn't touched.
        /// </summary>
        /// <returns>True always.</returns>
        private bool ConsumeOptionalRuleFunction()
        {
            if (tokenIterator.HasNextType(TokenType.AtIdentifier))
            {
                Token funcToken = tokenIterator.Next();
                Assert(funcToken.Text != "" && funcToken.Text != "@");
                ruleFunctionName = funcToken.Text;
            }

            return true;
        }

        /// <summary>
        /// Consumes an expression. Stream may not be returned to its original
        /// marked state.
        /// </summary>
        /// <returns>True if an expression was consumed, false otherwise.
        /// </returns>
        private bool ConsumeExpression()
        {
            return ConsumeOneOrMoreAny(ConsumeRepeatableExpression, ConsumeOptionalExpression);
        }

        /// <summary>
        /// Consumes a repeatable expression. Rolls back on failure to preserve
        /// the token stream.
        /// </summary>
        /// <returns>True on success, false if not.</returns>
        private bool ConsumeRepeatableExpression()
        {
            bool foundExpr = ConsumeOneOrMoreAny(ConsumeWord,
                                                 ConsumeQuotedWord,
                                                 ConsumeVariable,
                                                 ConsumeChoicesExpression,
                                                 ConsumeOptionalExpression);
            if (foundExpr)
            {
                ConsumeOptionalRepeatable();
            }

            return foundExpr;
        }

        /// <summary>
        /// Consumes a repeatable element if any, rewinds if not found.
        /// </summary>
        /// <returns>True if a repeatable definition was found, false 
        /// otherwise.</returns>
        private bool ConsumeOptionalRepeatable()
        {
            ConsumeAny(ConsumeRepeat, ConsumeKleeneStar, ConsumeKleenePlus);
            return true;
        }

        /// <summary>
        /// Consumes a repeat in {min[,[max]]} form, where the comma and max
        /// value are optional. Rewinds on failure.
        /// </summary>
        /// <returns>True on successful consumption, false if not.</returns>
        private bool ConsumeRepeat()
        {
            // Note that the min/max values are set by the functions below.
            // These functions set the max/min value and do sanity checks.
            // This means CompilerExceptions may be thrown here.
            bool validConsume = ConsumeAllSequentially(ConsumeLeftCurlyBrace,
                                                       ConsumeRepeatFirstNumber,
                                                       ConsumeRepeatOptionalComma,
                                                       ConsumeRepeatOptionalSecondNumber,
                                                       ConsumeRightCurlyBrace);
            if (validConsume)
            {
                // Note that the ranges are handled inside the above functions,
                // therefore there should not be any exceptions thrown here.
                currentListBuilder.SetRangeOfMostRecentNode(lastMinRange, lastMaxRange);
            }

            return validConsume;
        }

        /// <summary>
        /// Consumes the first nubmer in the repeat sequence. Rolls back on
        /// failure.
        /// </summary>
        /// <returns>True on success, false otherwise.</returns>
        /// <exception cref="CompilerException">If the number is negative.
        /// </exception>
        private bool ConsumeRepeatFirstNumber()
        {
            if (!tokenIterator.HasNextType(TokenType.Number))
            {
                return false;
            }

            int marker = tokenIterator.GetMarker();
            int value = 0;
            Token token = tokenIterator.Next();

            if (!int.TryParse(token.Text, out value))
            {
                tokenIterator.SetToMarker(marker);
                return false;
            }

            if (value < 0)
            {
                throw new CompilerException($"Cannot have a negative repeat value (line {token.LineNumber}).");
            }

            lastMinRange = value;
            lastMaxRange = value; // In case its just {num} then clamp it.
            return true;
        }

        /// <summary>
        /// Consumes an optional comma if it exists. The stream is restored if
        /// there is any failure. Sets the max value to the max int, which is
        /// overridden by an optional number if it exists later.
        /// </summary>
        /// <returns>Always true.</returns>
        private bool ConsumeRepeatOptionalComma()
        {
            // If the comma exists, assume it's the max value. This way if
            // there is not a number after, all our work is done already.
            if (ConsumeTokenTypeIfPresent(TokenType.Comma))
            {
                lastMaxRange = int.MaxValue;
            }

            return true;
        }

        /// <summary>
        /// Attempts to consume an optional number. Preserves the stream by
        /// rewinding on failure if it occurs.
        /// </summary>
        /// <returns>True always.</returns>
        /// <exception cref="CompilerException">If the max number parsed is
        /// less than the minimum number read in earlier.</exception>
        private bool ConsumeRepeatOptionalSecondNumber()
        {
            if (!tokenIterator.HasNextType(TokenType.Number))
            {
                return true;
            }

            int marker = tokenIterator.GetMarker();
            int value = 0;
            Token token = tokenIterator.Next();

            if (!int.TryParse(token.Text, out value))
            {
                tokenIterator.SetToMarker(marker);
                return true;
            }

            if (value < lastMinRange)
            {
                throw new CompilerException($"Max value is less than the paired minimum value (line {token.LineNumber}).");
            }

            lastMaxRange = value;
            return true;
        }

        /// <summary>
        /// Consumes the kleene star if it exists, and sets the immediate node
        /// on the stack to repeat 0 -> infinity. Rewinds if there is no token
        /// or advances if it finds it.
        /// </summary>
        /// <returns>True if found, false if not.</returns>
        private bool ConsumeKleeneStar()
        {
            if (tokenIterator.HasNextType(TokenType.Star))
            {
                tokenIterator.Next();
                currentListBuilder.SetRangeOfMostRecentNode(0, int.MaxValue);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Consumes the kleene plus if it exists, and sets the immediate node
        /// on the stack to repeat 1 -> infinity. Rewinds if there is no token
        /// or advances if it finds it.
        /// </summary>
        /// <returns>True if found, false if not.</returns>
        private bool ConsumeKleenePlus()
        {
            if (tokenIterator.HasNextType(TokenType.Plus))
            {
                tokenIterator.Next();
                currentListBuilder.SetRangeOfMostRecentNode(1, int.MaxValue);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Consumes a pipe expression, rewinds the stream on failure.
        /// </summary>
        /// <returns>True if it read a pipe expression, false otherwise.
        /// </returns>
        private bool ConsumePipeExpression()
        {
            return ConsumeAllSequentially(ConsumeExpression, ConsumeOptionalPipeAndExpressions);
        }

        /// <summary>
        /// Consumes any remaining pipe and expression chains. This means
        /// multiple sequences like "| expr | expr", etc, are consumed.
        /// </summary>
        /// <returns>True always.</returns>
        private bool ConsumeOptionalPipeAndExpressions()
        {
            bool foundExpr = true;
            while (foundExpr)
            {
                int marker = tokenIterator.GetMarker();
                foundExpr = ConsumeAllSequentially(ConsumePipeToken, ConsumeExpression);

                if (!foundExpr)
                {
                    tokenIterator.SetToMarker(marker);
                }
            }

            return true;
        }

        /// <summary>
        /// Consumes the pipe token and handles the effect on the nodes for the
        /// node stack.
        /// </summary>
        /// <returns>True if a pipe was found and consumed, false otherwise.
        /// </returns>
        private bool ConsumePipeToken()
        {
            if (!tokenIterator.HasNextType(TokenType.Pipe))
            {
                return false;
            }

            tokenIterator.Next();
            currentListBuilder.HandlePipeOption();

            return true;
        }

        /// <summary>
        /// Consumes a choices expression and all its internal expressions.
        /// </summary>
        /// <returns>True if a choices expression was consumed, false if not.
        /// </returns>
        private bool ConsumeChoicesExpression()
        {
            if (!tokenIterator.HasNextType(TokenType.ParenStart))
            {
                return false;
            }

            int marker = tokenIterator.GetMarker();
            tokenIterator.Next();
            currentListBuilder.StartNewChoices();

            if (!ConsumePipeExpression())
            {
                tokenIterator.SetToMarker(marker);
                return false;
            }

            if (!tokenIterator.HasNextType(TokenType.ParenEnd))
            {
                tokenIterator.SetToMarker(marker);
                return false;
            }

            tokenIterator.Next();
            currentListBuilder.EndChoices();

            return true;
        }

        /// <summary>
        /// Consumes a optional expression and all its internal expressions.
        /// </summary>
        /// <returns>True if a choices expression was consumed, false if not.
        /// </returns>
        private bool ConsumeOptionalExpression()
        {
            if (!tokenIterator.HasNextType(TokenType.BracketStart))
            {
                return false;
            }

            int marker = tokenIterator.GetMarker();
            tokenIterator.Next();
            currentListBuilder.StartNewOptionalChoices();

            if (!ConsumePipeExpression())
            {
                tokenIterator.SetToMarker(marker);
                return false;
            }

            if (!tokenIterator.HasNextType(TokenType.BracketEnd))
            {
                tokenIterator.SetToMarker(marker);
                return false;
            }

            tokenIterator.Next();
            currentListBuilder.EndOptionalChoices();

            return true;
        }

        /// <summary>
        /// Consumes a word.
        /// </summary>
        /// <returns>True if consumed, false if not.</returns>
        private bool ConsumeWord()
        {
            if (!tokenIterator.HasNextType(TokenType.Word))
            {
                return false;
            }

            Token token = tokenIterator.Next();
            currentListBuilder.AddNewWord(token.Text);

            return true;
        }

        /// <summary>
        /// Consumes a quoted word (or words).
        /// </summary>
        /// <returns>True if consumed, false if not.</returns>
        private bool ConsumeQuotedWord()
        {
            if (!tokenIterator.HasNextType(TokenType.QuotedString))
            {
                return false;
            }

            Token token = tokenIterator.Next();
            currentListBuilder.AddNewWord(token.Text);

            return true;
        }

        /// <summary>
        /// Consumes a variable and performs the required lookup/linking as
        /// needed.
        /// </summary>
        /// <returns>True if consumed, false if not.</returns>
        private bool ConsumeVariable()
        {
            if (!tokenIterator.HasNextType(TokenType.DollarIdentifier))
            {
                return false;
            }

            Token token = tokenIterator.Next();
            currentListBuilder.AddNewVariable(null /* TODO */);

            return true;
        }

        #endregion
    }
}
