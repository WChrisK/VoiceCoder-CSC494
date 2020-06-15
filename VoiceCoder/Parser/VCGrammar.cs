//  Extends the .NET Grammar object to hold extra data.
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

using System.IO;
using System.Speech.Recognition;
using static VoiceCoder.Util.Assertion;

namespace VoiceCoder.Parser
{
    /// <summary>
    /// An extended version of the .NET grammar, holds information for looking
    /// up metadata for Python calls.
    /// </summary>
    public class VCGrammar : Grammar
    {
        /// <summary>
        /// The python function to be called, if any.
        /// </summary>
        public string PythonFunction { get; private set; }

        /// <summary>
        /// The file path for where the this function can be found.
        /// </summary>
        public string PythonFilePath { get; private set; }

        // TODO
        public VCGrammar(GrammarBuilder builder) : base(builder)
        {
            PythonFunction = "";
            PythonFilePath = "";
        }

        // TODO
        public void SetPythonFunction(string functionName)
        {
            CheckNotNull(functionName);
            PythonFunction = functionName;
        }

        // TODO
        public void SetPythonFilePath(string fullPath)
        {
            CheckNotNull(fullPath);
            CheckArgument(File.Exists(fullPath));
            PythonFilePath = fullPath;
        }
    }
}
