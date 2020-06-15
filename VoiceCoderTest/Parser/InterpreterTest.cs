//  Tests the Compiler.
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
using System.Collections.ObjectModel;
using System.IO;
using System.Speech.Recognition;
using VoiceCoder.Parser;

namespace VoiceCoderTest.Parser
{
    [TestClass]
    public class InterpreterTest
    {
        /// <summary>
        /// The location of the testing folder.
        /// </summary>
        private readonly string RESOURCE_TEST_FOLDER = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\Resources\CompilerTest";

        [TestMethod]
        public void TestInterpreterOnFolder()
        {
            Interpreter interpreter = new Interpreter();
            interpreter.AddFilesFromDirectory(RESOURCE_TEST_FOLDER);
            interpreter.Compile();

            // Test modules.
            Assert.AreEqual(3, interpreter.ModuleMap.Count);
            Assert.IsTrue(interpreter.ModuleMap.ContainsKey("outer"));
            Assert.IsTrue(interpreter.ModuleMap.ContainsKey("package.test"));
            Assert.IsTrue(interpreter.ModuleMap.ContainsKey("package.inner.inner"));

            // Test imports.
            Module module;
            ReadOnlyDictionary<string, Tuple<string, bool>> moduleImportDict;

            module = interpreter.ModuleMap["outer"];
            moduleImportDict = module.LoadedImports;
            Assert.AreEqual(1, moduleImportDict.Count);
            Assert.IsTrue(moduleImportDict.ContainsKey("package"));
            Assert.AreEqual("", moduleImportDict["package"].Item1);
            Assert.IsTrue(moduleImportDict["package"].Item2);

            module = interpreter.ModuleMap["package.test"];
            moduleImportDict = module.LoadedImports;
            Assert.AreEqual(1, moduleImportDict.Count);
            Assert.IsTrue(moduleImportDict.ContainsKey("package.inner"));
            Assert.AreEqual("yes", moduleImportDict["package.inner"].Item1);
            Assert.IsFalse(moduleImportDict["package.inner"].Item2);

            // Test compilation.
            module = interpreter.ModuleMap["package.test"]; // Redundant, but futureproofing.
            Assert.AreEqual("a* (b{3} c+ | (d | e{2,9}) [f]) ((g)) h{3,}", module.LoadedGrammarNodes["test"].ToString());
            GrammarBuilder gb = GrammarCompiler.CompileToGrammarBuilder(module.LoadedGrammarNodes["test"]);
            Assert.AreEqual("‘a’ [‘b’ ‘c’,[‘d’,‘e’] [‘f’]] [[‘g’]] ‘h’", gb.DebugShowPhrases);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullInterpreterArgs()
        {
            new Interpreter().AddFilesFromDirectory(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidFolder()
        {
            new Interpreter().AddFilesFromDirectory("fake folder");
        }
    }
}
