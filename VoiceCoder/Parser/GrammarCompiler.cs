//  Compiles a Hierarchial Node root into a .NET grammar object.
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

using System.Speech.Recognition;
using static System.Diagnostics.Debug;
using static VoiceCoder.Util.Assertion;

namespace VoiceCoder.Parser
{
    /// <summary>
    /// Compiles a root node into a useful .NET grammar object.
    /// </summary>
    public class GrammarCompiler
    {
        /// <summary>
        /// The grammar object that will be built.
        /// </summary>
        private GrammarBuilder grammarBuilder;

        /// <summary>
        /// Creates a grammar object from a root node.
        /// </summary>
        /// <param name="rootNode">The node to compile from.</param>
        private GrammarCompiler(HierarchicalNode rootNode)
        {
            Assert(rootNode != null);
            grammarBuilder = CompileNodeRecursively(rootNode);
        }

        /// <summary>
        /// Compiles the node recursively, meaning its children are compiled
        /// and its next link (if any exists) is also compiled.
        /// </summary>
        /// <param name="node">The node to recursively compile.</param>
        /// <returns>The grammar builder of this object and the children and/or
        /// the next node all recursively.</returns>
        private GrammarBuilder CompileNodeRecursively(HierarchicalNode node)
        {
            Assert(node != null);

            GrammarBuilder grammarBuilderCurrent = null;
            GrammarBuilder grammarBuilderNext = null;

            // If there's another element after this, compile that recursively first.
            if (node.Next != null)
            {
                grammarBuilderNext = CompileNodeRecursively(node.Next);
            }

            // If there's children, compile that; otherwise compile itself.
            if (node.Children.Count > 0)
            {
                Choices choices = new Choices();
                foreach (HierarchicalNode childNode in node.Children)
                {
                    choices.Add(CompileNodeRecursively(childNode));
                }
                grammarBuilderCurrent = new GrammarBuilder(choices, node.MinRepeat, node.MaxRepeat);
            }
            else
            {
                grammarBuilderCurrent = new GrammarBuilder(node.Value, node.MinRepeat, node.MaxRepeat);
            }

            // If there was a next node, link the next node after this one.
            if (grammarBuilderNext != null)
            {
                grammarBuilderCurrent.Append(grammarBuilderNext);
            }

            return grammarBuilderCurrent;
        }

        /// <summary>
        /// Compiles a root node to a .NET grammar object.
        /// </summary>
        /// <param name="rootNode">The node to compile from.</param>
        /// <returns>A .NET Grammar object that can be loaded into the speech
        /// recognition engine.</returns>
        /// <exception cref="ArgumentNullException">If the argument is null.
        /// </exception>
        public static GrammarBuilder CompileToGrammarBuilder(HierarchicalNode rootNode)
        {
            CheckNotNull(rootNode);
            return new GrammarCompiler(rootNode).grammarBuilder;
        }
    }
}
