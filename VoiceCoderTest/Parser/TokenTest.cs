//  Tests the token class for any errors.
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
using System;
using VoiceCoder.Parser;

namespace VoiceCoderTest
{
    [TestClass]
    public class TokenTest
    {
        [TestMethod]
        public void TestValidTokens()
        {
            Token token = new Token(TokenType.Word, "Te" + "st", 1, 3);
            Assert.AreEqual(token.Type, TokenType.Word);
            Assert.AreEqual(token.Text, "Test", false);
            Assert.AreEqual(token.LineNumber, 1);
            Assert.AreEqual(token.CharOffset, 3);

            token = new Token(TokenType.AtIdentifier, "up_to_the_USER.", 42, 89);
            Assert.AreEqual(token.Type, TokenType.AtIdentifier);
            Assert.AreEqual(token.Text, "up_to_the_USER.", false);
            Assert.AreEqual(token.LineNumber, 42);
            Assert.AreEqual(token.CharOffset, 89);
        }

        [TestMethod]
        public void TestCopyConstructor()
        {
            Token token = new Token(TokenType.Word, "Te" + "st", 1, 3);
            Token newToken = new Token(token);
            Assert.AreEqual(newToken.Type, TokenType.Word);
            Assert.AreEqual(newToken.Text, "Test", false);
            Assert.AreEqual(newToken.LineNumber, 1);
            Assert.AreEqual(newToken.CharOffset, 3);
            Assert.AreEqual(newToken, token);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullText()
        {
            new Token(TokenType.AtIdentifier, null, 2, 4);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestNegativeLineNumber()
        {
            new Token(TokenType.AngleStart, "test", -1, 4);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestNegativeCharOffset()
        {
            new Token(TokenType.AngleStart, "test", 0, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestEmptyString()
        {
            new Token(TokenType.AngleStart, "", 1, 4);
        }

        [TestMethod]
        public void TestMultipleBadInput()
        {
            try
            {
                new Token(TokenType.DollarIdentifier, null, -41, -38);
                Assert.Fail();
            }
            catch (Exception e) when (e is ArgumentException || e is ArgumentNullException)
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBadTokenType()
        {
            new Token(TokenType.NONE, "hi", 2, 4);
        }

        [TestMethod]
        public void TestEquals()
        {
            Token same1 = new Token(TokenType.AtIdentifier, "b", 1, 2);
            Token same2 = new Token(TokenType.AtIdentifier, "b", 1, 2);
            Token reference = same1;
            Token copyCtor = new Token(same1);
            Token different = new Token(TokenType.AngleEnd, ">", 0, 58);
            Token differentType = new Token(TokenType.ParenEnd, "@", 1, 2);
            Token differentText = new Token(TokenType.AtIdentifier, "a", 1, 2);
            Token differentLine = new Token(TokenType.AtIdentifier, "a", 123, 2);
            Token differentCharOffset = new Token(TokenType.AtIdentifier, "a", 1, 53);

            Assert.IsTrue(same1.Equals(same2));
            Assert.IsTrue(same1.Equals(reference));
            Assert.IsTrue(same1.Equals(copyCtor));
            Assert.IsFalse(same1.Equals(different));
            Assert.IsFalse(same1.Equals(differentText));
            Assert.IsFalse(same1.Equals(differentType));
            Assert.IsFalse(same1.Equals(differentLine));
            Assert.IsFalse(same1.Equals(differentCharOffset));
        }

        [TestMethod]
        public void TestToString()
        {
            Token token = new Token(TokenType.Word, "Ab", 4, 2);
            string expected = "<Token[Type=Word,Text=Ab,LineNumber=4,CharOffset=2]>";
            Assert.AreEqual(token.ToString(), expected, false);
        }
    }
}
