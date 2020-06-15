//  Tests the TokenizerException.
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
using VoiceCoder.Parser;

namespace VoiceCoderTest.Parser
{
    [TestClass]
    public class TokenizerExceptionTest
    {
        [TestMethod]
        public void TestExceptionLineChar()
        {
            TokenizerException te = new TokenizerException(5, 42);
            Assert.AreEqual(5, te.LineNumber);
            Assert.AreEqual(42, te.CharOffset);

            te = new TokenizerException(51, 22, "my msg");
            Assert.AreEqual(51, te.LineNumber);
            Assert.AreEqual(22, te.CharOffset);
            Assert.AreEqual("my msg", te.Message, false);
        }
    }
}
