//  Tests the TokenIterator.
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VoiceCoder.Parser;

namespace VoiceCoderTest.Parser
{
    [TestClass]
    public class TokenIteratorTest
    {
        [TestMethod]
        public void TestEmptyIterator()
        {
            TokenIterator tokenIterator = new TokenIterator(new List<Token>());
            Assert.IsFalse(tokenIterator.HasNext());
            Assert.AreEqual(0, tokenIterator.Count);
            int marker = tokenIterator.GetMarker();
            Assert.AreEqual(0, marker);
            Assert.AreEqual(0, tokenIterator.GetMarker());
        }

        [TestMethod]
        public void TestIterator()
        {
            List<Token> tokens = new List<Token>();
            Token tokenReference = new Token(TokenType.DollarIdentifier, "a", 5, 2);
            tokens.Add(tokenReference);
            tokens.Add(new Token(TokenType.Word, "hi", 5, 3));
            tokens.Add(new Token(TokenType.AtIdentifier, "b", 12, 0));
            tokens.Add(new Token(TokenType.Word, "yEs", 14, 42));
            TokenIterator tokenIterator = new TokenIterator(tokens);

            Assert.IsTrue(tokenIterator.HasNext());
            Assert.AreEqual(4, tokenIterator.Count);

            // Compare references to make sure we're getting back what we add.
            Token token = tokenIterator.Next();
            Assert.AreEqual(tokenReference, token);

            token = tokenIterator.Next();
            Assert.AreEqual(TokenType.Word, token.Type);
            Assert.AreEqual("hi", token.Text, false);
            Assert.AreEqual(5, token.LineNumber);
            Assert.AreEqual(3, token.CharOffset);

            int marker = tokenIterator.GetMarker();
            Assert.AreEqual(2, marker);

            Assert.IsTrue(tokenIterator.HasNext());

            token = tokenIterator.Next();
            Assert.AreEqual(TokenType.AtIdentifier, token.Type);
            Assert.AreEqual("b", token.Text, false);
            Assert.AreEqual(12, token.LineNumber);
            Assert.AreEqual(0, token.CharOffset);

            Assert.IsTrue(tokenIterator.HasNext());

            token = tokenIterator.Next();
            Assert.AreEqual(TokenType.Word, token.Type);
            Assert.AreEqual("yEs", token.Text, false);
            Assert.AreEqual(14, token.LineNumber);
            Assert.AreEqual(42, token.CharOffset);

            Assert.IsFalse(tokenIterator.HasNext());

            // Return to the second token.
            tokenIterator.SetToMarker(marker);

            Assert.IsTrue(tokenIterator.HasNext());

            token = tokenIterator.Next();
            Assert.AreEqual(TokenType.AtIdentifier, token.Type);
            Assert.AreEqual("b", token.Text, false);
            Assert.AreEqual(12, token.LineNumber);
            Assert.AreEqual(0, token.CharOffset);
        }

        [TestMethod]
        public void TestHasNext()
        {
            List<Token> tokens = new List<Token>();
            tokens.Add(new Token(TokenType.DollarIdentifier, "a", 5, 2));
            tokens.Add(new Token(TokenType.Word, "a", 5, 3));
            tokens.Add(new Token(TokenType.AtIdentifier, "@b", 12, 0));
            tokens.Add(new Token(TokenType.Word, "c", 14, 42));
            TokenIterator tokenIterator = new TokenIterator(tokens);

            Assert.IsTrue(tokenIterator.HasNextType(TokenType.DollarIdentifier));
            Assert.IsTrue(tokenIterator.HasNextType(TokenType.DollarIdentifier, TokenType.Pipe));
            Assert.IsTrue(tokenIterator.HasNextType(TokenType.Period, TokenType.DollarIdentifier, TokenType.Pipe));
            Assert.IsFalse(tokenIterator.HasNextType(TokenType.Number));
            tokenIterator.Next();
            tokenIterator.Next();
            tokenIterator.Next();
            tokenIterator.Next();
            Assert.IsFalse(tokenIterator.HasNextType(TokenType.Word));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullConstructor()
        {
            new TokenIterator(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestBadNext()
        {
            TokenIterator tokenIterator = new TokenIterator(new List<Token>());
            tokenIterator.Next();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestBadTwoNext()
        {
            List<Token> tokens = new List<Token>();
            tokens.Add(new Token(TokenType.AtIdentifier, "asd", 0, 0));
            TokenIterator tokenIterator = new TokenIterator(tokens);
            try
            {
                tokenIterator.Next();
            }
            catch (ArgumentOutOfRangeException)
            {
                // First one is valid, it shouldn't fail here.
                Assert.Fail();
            }

            tokenIterator.Next();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestBadMarker1()
        {
            TokenIterator tokenIterator = new TokenIterator(new List<Token>());
            // Markers don't work when you have no elements.
            tokenIterator.SetToMarker(tokenIterator.GetMarker());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestBadMarker2()
        {
            List<Token> tokens = new List<Token>();
            tokens.Add(new Token(TokenType.AtIdentifier, "at", 0, 0));
            TokenIterator tokenIterator = new TokenIterator(tokens);
            tokenIterator.SetToMarker(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestBadMarker3()
        {
            List<Token> tokens = new List<Token>();
            tokens.Add(new Token(TokenType.AtIdentifier, "at", 0, 0));
            TokenIterator tokenIterator = new TokenIterator(tokens);
            tokenIterator.SetToMarker(1);
        }
    }
}
