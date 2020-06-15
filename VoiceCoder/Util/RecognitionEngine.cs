//  Encapsulation of the recognition engine and IronPython engine.
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

using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System;
using System.Diagnostics;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using VoiceCoder.Parser;

namespace VoiceCoder.Util
{
    /// <summary>
    /// The main recognition engine for detecting speech and calling the
    /// appropriate scripts.
    /// </summary>
    public class RecognitionEngine : IDisposable
    {
        private SpeechRecognitionEngine speechRecognitionEngine;

        private SpeechSynthesizer speechSynthesizer;

        private ScriptEngine pythonEngine;

        public RecognitionEngine()
        {
            speechRecognitionEngine = new SpeechRecognitionEngine();
            speechRecognitionEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);
            speechRecognitionEngine.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(SpeechNotRecognized);

            speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.Rate = 1;
            speechSynthesizer.SelectVoiceByHints(VoiceGender.Female);

            pythonEngine = Python.CreateEngine();
        }

        public void LoadFolder(string folderPath)
        {
            Interpreter interpreter = new Interpreter();
            interpreter.AddFilesFromDirectory(folderPath);
            interpreter.Compile();
            foreach (VCGrammar grammar in interpreter.CompiledGrammar)
            {
                speechRecognitionEngine.LoadGrammar(grammar);
            }
            speechRecognitionEngine.RequestRecognizerUpdate();
            speechRecognitionEngine.EmulateRecognize("hello");
        }

        public void StartListening()
        {
            speechRecognitionEngine.SetInputToDefaultAudioDevice();
            speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void SpeechNotRecognized(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Debug.WriteLine("Unknown speech.");
        }

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Debug.WriteLine("Found speech: " + e.Result.Text);
            if (e.Result.Grammar is VCGrammar)
            {
                VCGrammar vcGrammar = (VCGrammar) e.Result.Grammar;
                if (vcGrammar.PythonFunction != "" && vcGrammar.PythonFilePath != "")
                {
                    dynamic pythonScope = pythonEngine.CreateScope();
                    pythonEngine.ExecuteFile(vcGrammar.PythonFilePath, pythonScope);
                    // TODO - check if the function exists before calling it
                    var input = pythonEngine.Execute(vcGrammar.PythonFunction + "()", pythonScope);
                    string inputStr = input.ToString();
                    Debug.WriteLine(">>> " + inputStr);
                    //Native.EmitKeys(input); // TODO - Check sanitization
                }
            }
        }

        public void Dispose()
        {
            speechRecognitionEngine.RecognizeAsyncStop();
            speechSynthesizer.Dispose();
            speechRecognitionEngine.UnloadAllGrammars();
            speechRecognitionEngine.Dispose();
        }
    }
}
