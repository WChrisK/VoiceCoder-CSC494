//  Tests the assertion class for any errors.
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
using static VoiceCoder.Util.Assertion;

namespace VoiceCoderTest.Util
{
    [TestClass]
    public class AssertionTest
    {
        [TestMethod]
        public void TestNotNull()
        {
            CheckNotNull("valid obj", "message");
        }

        [TestMethod]
        public void TestNotNullWithNullMessage()
        {
            CheckNotNull("valid obj", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNull()
        {
            CheckNotNull(null);
        }

        [TestMethod]
        public void TestNullMessage()
        {
            try
            {
                CheckNotNull(null, "my _ message");
                Assert.Fail();
            }
            catch (ArgumentNullException e)
            {
                Assert.AreEqual("my _ message", e.Message, false);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullWithNullMessage()
        {
            CheckNotNull(null, null);
        }

        [TestMethod]
        public void TestArgument()
        {
            CheckArgument(5 + 4 > 8);
            CheckArgument(true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidArgument()
        {
            CheckArgument(4 == 1);
        }

        [TestMethod]
        public void TestInvalidArgumentMessage()
        {
            try
            {
                CheckArgument(4 == 1, "my reason");
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("my reason", e.Message, false);
            }
        }

        [TestMethod]
        public void TestArgumentWithNullMessage()
        {
            CheckNotNull(false, null);
        }
    }
}
