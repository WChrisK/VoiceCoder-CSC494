//  Interprets a VCG file into usable grammar objects.
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using static System.Diagnostics.Debug;
using static VoiceCoder.Util.Assertion;

namespace VoiceCoder.Parser
{
    /// <summary>
    /// Compiles a stream of tokens into grammar recognition objects that can
    /// be interpreted by .NET's speech recognition.
    /// </summary>
    public class Interpreter
    {
        /// <summary>
        /// A list of end of folder characters.
        /// </summary>
        private readonly char[] TERMINATING_SLASHES = new char[] { '\\', '/' };

        /// <summary>
        /// The lookup of package/name to module. The format is:
        /// "package.here.name" => module
        /// </summary>
        public Dictionary<string, Module> ModuleMap { get; private set; }

        /// <summary>
        /// A list of all the compiled grammar. Will be empty if no compilation
        /// is carried out.
        /// </summary>
        public List<VCGrammar> CompiledGrammar;

        /// <summary>
        /// Creates an interpreter with no modules.
        /// </summary>
        public Interpreter()
        {
            ModuleMap = new Dictionary<string, Module>();
            CompiledGrammar = new List<VCGrammar>();
        }

        /// <summary>
        /// Adds all the .vcg files from the directory provided recursively.
        /// </summary>
        /// <param name="directoryRootPath">The root directory path.</param>
        /// <exception cref="ArgumentNullException">If the path is null.
        /// </exception>
        /// <exception cref="ArgumentException">If the folder is not a valid
        /// path and/or does not exist.</exception>
        /// <exception cref="IOException">If there are any problems reading or
        /// traversing a directory/file tree from the provided directory path.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">If access is denied
        /// when trying to walk the tree.</exception>
        /// <exception cref="SecurityException">If the walking yields a 
        /// security exception (permissions are denied).</exception>
        public void AddFilesFromDirectory(string directoryRootPath)
        {
            CheckNotNull(directoryRootPath);
            CheckArgument(Directory.Exists(directoryRootPath));

            DirectoryInfo directoryInfo = new DirectoryInfo(directoryRootPath);
            WalkDirectoryForFiles(directoryInfo.FullName, directoryInfo);
        }

        /// <summary>
        /// Takes a vcg file full path and converts it to a python path.
        /// NOTE: Does not support anything other than lowercase extensions.
        /// </summary>
        /// <param name="fullPathVcg">The full path to the file.</param>
        /// <param name="fileNameNoExt">The name of the file (included in the
        /// full path as well).</param>
        /// <returns>The python path from the vcg file path</returns>
        private string GetPythonPathFromFullPath(string fullPathVcg, string fileNameNoExt)
        {
            Assert(fullPathVcg != null);
            Assert(fullPathVcg.Length > 0);

            string pythonPath = "";
            int lastIndex = fullPathVcg.LastIndexOfAny(TERMINATING_SLASHES);
            if (lastIndex >= 0)
            {
                pythonPath = fullPathVcg.Substring(0, lastIndex + 1) + fileNameNoExt + ".py";
            }

            return pythonPath;
        }

        /// <summary>
        /// Walks the directory and reads in the required files.
        /// </summary>
        /// <param name="directoryRootPath">The root path of the directory.
        /// </param>
        /// <param name="directoryInfo">The directory information to use when
        /// walking.</param>
        /// <exception cref="IOException">If there are any problems reading or
        /// traversing a directory/file tree from the provided directory path.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">If access is denied
        /// when trying to walk the tree.</exception>
        private void WalkDirectoryForFiles(string directoryRootPath, DirectoryInfo directoryInfo)
        {
            Assert(directoryRootPath != null);
            Assert(directoryRootPath.Length > 0);
            Assert(directoryInfo != null);

            FileInfo[] files = directoryInfo.GetFiles("*.vcg");
            if (files != null)
            {
                foreach (FileInfo fileInfo in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                    string relativeDirToFile = fileInfo.DirectoryName.Substring(directoryRootPath.Length);

                    // For non-empty relative paths, a slash is left over at the beginning.
                    // Remove that if it exists, otherwise just leave it as a blank package.
                    if (relativeDirToFile.Length > 0 && relativeDirToFile[0] == '\\')
                    {
                        relativeDirToFile = relativeDirToFile.Substring(1);
                    }

                    string pythonPath = GetPythonPathFromFullPath(fileInfo.FullName, fileName);
                    string packageName = relativeDirToFile.Replace(@"\", ".");
                    string referenceName = packageName + (packageName.Length > 0 ? "." : "") + fileName;

                    Module module = new Module(packageName, fileName, fileInfo.FullName);
                    module.SetPythonFile(pythonPath); // Empty strings are allowed, so this is okay.
                    ModuleMap.Add(referenceName, module);
                }
            }

            DirectoryInfo[] directories = directoryInfo.GetDirectories();
            if (directories != null)
            {
                foreach (DirectoryInfo dirInfo in directories)
                {
                    WalkDirectoryForFiles(directoryRootPath, dirInfo);
                }
            }
        }

        /// <summary>
        /// Compiles all the loaded files into grammar definitions that can be
        /// loaded into the speech recognizer.
        /// </summary>
        /// <exception cref="TokenizerException">If there are any errors when
        /// tokenizing the files.</exception>
        /// <exception cref="CompilerException">If there are any errors when
        /// compiling the files.</exception>
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
        public void Compile()
        {
            DoTokenizePass();
            DoImportPass();
            DoRuleCompilationPass();
            DoGrammarCompilation();
        }

        /// <summary>
        /// Tokenizes the underlying file for this module.
        /// </summary>
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
        private void DoTokenizePass()
        {
            ModuleMap.Values.ToList().ForEach(m => m.Tokenize());
        }

        /// <summary>
        /// Performs an import pass over all tokens and assembles the import
        /// definitions only. Also makes sure the tokens are valid for VcgFile
        /// definitions. Must be run after the tokenizing pass.
        /// </summary>
        /// <exception cref="CompilerException">If there are any malformed
        /// import statements or double importing or corrupt definitions.
        /// </exception>
        private void DoImportPass()
        {
            ModuleMap.Values.ToList().ForEach(m => m.DoImportAndTokenValidityPass());
        }

        /// <summary>
        /// Compiles all the rules so the modules can be turned into grammar
        /// objects. Must be run after the import pass.
        /// </summary>
        /// <exception cref="CompilerException">If there are any malformed
        /// rules.</exception>
        private void DoRuleCompilationPass()
        {
            ModuleMap.Values.ToList().ForEach(m => m.DoRulePass());
        }

        /// <summary>
        /// Compiles all the parsed data into .NET speech recognition objects.
        /// </summary>
        private void DoGrammarCompilation()
        {
            foreach (string key in ModuleMap.Keys)
            {
                foreach (string nodeName in ModuleMap[key].LoadedGrammarNodes.Keys)
                {
                    HierarchicalNode rootNode = ModuleMap[key].LoadedGrammarNodes[nodeName];
                    GrammarBuilder grammarBuilder = GrammarCompiler.CompileToGrammarBuilder(rootNode);

                    VCGrammar grammar = new VCGrammar(grammarBuilder);
                    string pythonFilePath = ModuleMap[key].PythonFilePath;
                    if (pythonFilePath != null && pythonFilePath.Length > 0)
                    {
                        grammar.SetPythonFilePath(pythonFilePath);
                    }
                    grammar.Name = nodeName;

                    CompiledGrammar.Add(grammar);
                }
            }
        }
    }
}
