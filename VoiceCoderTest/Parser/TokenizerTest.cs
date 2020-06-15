//  Tests the Tokenizer.
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using VoiceCoder.Parser;

namespace VoiceCoderTest.Parser
{
    [TestClass]
    public class TokenizerTest
    {
        // Source for figuring this out:
        // http://stackoverflow.com/questions/816566/how-do-you-get-the-current-project-directory-from-c-sharp-code-when-creating-a-c
        private readonly string RESOURCE_TEST_FILE = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/Resources/Test.vcg";

        private readonly object[,] FILE_EXPECTED_TOKEN_ELEMENTS = new object[,]
        {
            { TokenType.Word, "import", 5, 0 },
            { TokenType.Word, "my", 5, 7 },
            { TokenType.Period, ".", 5, 9 },
            { TokenType.Word, "package", 5, 10 },
            { TokenType.Period, ".", 5, 17 },
            { TokenType.Word, "here", 5, 18 },
            { TokenType.Semicolon, ";", 5, 23 },
            { TokenType.Word, "import", 6, 0 },
            { TokenType.Word, "static", 6, 7 },
            { TokenType.Word, "the", 6, 14 },
            { TokenType.Period, ".", 6, 17 },
            { TokenType.Word, "import", 6, 18 },
            { TokenType.Period, ".", 6, 24 },
            { TokenType.Star, "*", 6, 25 },
            { TokenType.Semicolon, ";", 6, 26 },
            { TokenType.DollarIdentifier, "test", 8, 0 },
            { TokenType.Equals, "=", 8, 6 },
            { TokenType.Word, "hello", 8, 8 },
            { TokenType.BracketStart, "[", 8, 14 },
            { TokenType.Word, "my", 8, 15 },
            { TokenType.Word, "friendly", 8, 18 },
            { TokenType.BracketEnd, "]", 8, 26 },
            { TokenType.Word, "computer", 8, 28 },
            { TokenType.Number, "12", 8, 37 },
            { TokenType.Semicolon, ";", 8, 39 },
            { TokenType.DollarIdentifier, "some_thing", 10, 0 },
            { TokenType.AtIdentifier, "func", 10, 12 },
            { TokenType.Equals, "=", 10, 18 },
            { TokenType.Word, "yes", 10, 20 },
            { TokenType.BracketStart, "[", 10, 24 },
            { TokenType.ParenStart, "(", 10, 25 },
            { TokenType.Word, "and", 10, 26 },
            { TokenType.Pipe, "|", 10, 30 },
            { TokenType.Word, "or", 10, 32 },
            { TokenType.ParenEnd, ")", 10, 34 },
            { TokenType.QuotedString, "no", 10, 36 },
            { TokenType.BracketEnd, "]", 10, 40 },
            { TokenType.Semicolon, ";", 10, 41 }
        };

        [TestMethod]
        public void TestTokenizeFileWithIterator()
        {
            Tokenizer tokenizer = Tokenizer.FromFile(RESOURCE_TEST_FILE);
            TokenIterator it = tokenizer.GetTokenIterator();
            Token token;
            Token actualToken;
            int index = 0;
            Assert.IsTrue(FILE_EXPECTED_TOKEN_ELEMENTS.Length == it.Count * 4);
            while (it.HasNext())
            {
                token = it.Next();
                actualToken = new Token((TokenType)FILE_EXPECTED_TOKEN_ELEMENTS[index, 0],
                                        (string)FILE_EXPECTED_TOKEN_ELEMENTS[index, 1],
                                        (int)FILE_EXPECTED_TOKEN_ELEMENTS[index, 2],
                                        (int)FILE_EXPECTED_TOKEN_ELEMENTS[index, 3]);
                Assert.AreEqual(actualToken, token);
                index++;
            }
        }

        [TestMethod]
        public void TestValidInput()
        {
            new Tokenizer("hello[hi];");
            new Tokenizer("hello<12>\nyes = 5;");
            new Tokenizer("$heh_heh @rofl = this is a \"very evil\" test;");
        }

        [TestMethod]
        public void TestWordTokenizing()
        {
            Tokenizer tokenizer = new Tokenizer("   this is\t\ta  Test");
            List<Token> tokenList = tokenizer.GetTokens();
            Assert.AreEqual(4, tokenList.Count);
            Assert.AreEqual(new Token(TokenType.Word, "this", 1, 3), tokenList[0]);
            Assert.AreEqual(new Token(TokenType.Word, "is", 1, 8), tokenList[1]);
            Assert.AreEqual(new Token(TokenType.Word, "a", 1, 12), tokenList[2]);
            Assert.AreEqual(new Token(TokenType.Word, "Test", 1, 15), tokenList[3]);
        }

        [TestMethod]
        public void TestCommentConsumption()
        {
            Tokenizer tokenizer = new Tokenizer("#####\n# comment\nhi#\n\n#Test");
            List<Token> tokenList = tokenizer.GetTokens();
            Assert.AreEqual(1, tokenList.Count);
            Assert.AreEqual(new Token(TokenType.Word, "hi", 3, 0), tokenList[0]);
        }

        [TestMethod]
        public void TestValidIdentifiers()
        {
            Tokenizer tokenizer = new Tokenizer("$hello\n@func\n$yes.no.maybe");
            List<Token> tokenList = tokenizer.GetTokens();
            Assert.AreEqual(3, tokenList.Count);
            Assert.AreEqual(new Token(TokenType.DollarIdentifier, "hello", 1, 0), tokenList[0]);
            Assert.AreEqual(new Token(TokenType.AtIdentifier, "func", 2, 0), tokenList[1]);
            Assert.AreEqual(new Token(TokenType.DollarIdentifier, "yes.no.maybe", 3, 0), tokenList[2]);
        }

        [TestMethod]
        public void TestValidNumbers()
        {
            Tokenizer tokenizer = new Tokenizer("123\n  45 \n\t5\t");
            List<Token> tokenList = tokenizer.GetTokens();
            Assert.AreEqual(3, tokenList.Count);
            Assert.AreEqual(new Token(TokenType.Number, "123", 1, 0), tokenList[0]);
            Assert.AreEqual(new Token(TokenType.Number, "45", 2, 2), tokenList[1]);
            Assert.AreEqual(new Token(TokenType.Number, "5", 3, 1), tokenList[2]);
        }

        [TestMethod]
        public void TestValidSymbolsAfterNumber()
        {
            string input = "12\n" +
                           "1 \n" +
                           "1)\n" +
                           "1(\n" +
                           "1>\n" +
                           "1>\n" +
                           "1[\n" +
                           "1|\n" +
                           "1=\n" +
                           "1{\n" +
                           "1}\n" +
                           "1\r\n" +
                           "1;\n" +
                           "1\t\n";
            new Tokenizer(input);
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestInvalidStandaloneUnderscore()
        {
            new Tokenizer("_");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestInvalidNumberWithLetter()
        {
            new Tokenizer("123 \n  4a5 \n\t5\t");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestInvalidNumberDecimalPoint()
        {
            new Tokenizer("123 a\n  4.5 \n\t5\t");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestInvalidNumberRandomSymbol()
        {
            new Tokenizer("1_");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestMissingQuoteEOL()
        {
            new Tokenizer("\"hi\"\n\"hello\nyes");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestMissingQuoteEOF()
        {
            new Tokenizer("\"hello");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestIllegalWord1()
        {
            new Tokenizer("hel$lo");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestIllegalWord2()
        {
            new Tokenizer("hel1");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestIllegalWord3()
        {
            new Tokenizer("hel_lo");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestIllegalWord4()
        {
            new Tokenizer("TE&ST");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestIllegalWord5()
        {
            new Tokenizer("TE_ST");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestIllegalWord6()
        {
            new Tokenizer("something rand123 hi");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestIllegalNumber1()
        {
            new Tokenizer("1234a");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestIllegalNumber2()
        {
            new Tokenizer("1234_ 12345");
        }

        [TestMethod]
        [ExpectedException(typeof(TokenizerException))]
        public void TestIllegalNumber3()
        {
            new Tokenizer("12$3");
        }
    }
}
