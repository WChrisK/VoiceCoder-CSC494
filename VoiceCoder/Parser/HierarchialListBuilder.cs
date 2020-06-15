//  Allows compiling of a chain of nodes for easy compiling to .NET grammar.
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
using static System.Diagnostics.Debug;
using static VoiceCoder.Util.Assertion;

namespace VoiceCoder.Parser
{
    /// <summary>
    /// An assisting class in designing the hierarchial list of nodes.
    /// </summary>
    public class HierarchialListBuilder
    {
        /// <summary>
        /// The name of the rule that uniquely identifies this.
        /// </summary>
        public string RuleIdentifier { get; private set; }

        /// <summary>
        /// If the next node should be added to the element on the choice
        /// stack as a child next or not.
        /// </summary>
        private bool addToChoiceAsChild;

        /// <summary>
        /// A starting node that is used only to start the chain additions.
        /// It will not be included as part of the final node.
        /// </summary>
        private HierarchicalNode startingDummyNode;

        /// <summary>
        /// True if the dummy node has been removed, false otherwise.
        /// </summary>
        private bool removedDummyNode;

        /// <summary>
        /// The root node in this hierarchy.
        /// </summary>
        private HierarchicalNode rootNode;

        /// <summary>
        /// A stack of all the choices, where the most recent choice appears
        /// on the top of the stack.
        /// </summary>
        private Stack<HierarchicalNode> choicesNodeStack;

        /// <summary>
        /// A list of nodes to be appended to as a list.
        /// </summary>
        private Stack<HierarchicalNode> chainNodeStack;

        /// <summary>
        /// Creates an empty list that can be added to.
        /// </summary>
        /// <exception cref="ArgumentNullException>">If the rule name is null.
        /// </exception>
        /// <exception cref="ArgumentException">If the rule name is empty.
        /// </exception>
        public HierarchialListBuilder(string ruleIdentifier)
        {
            CheckNotNull(ruleIdentifier);
            CheckArgument(ruleIdentifier.Length > 0);

            RuleIdentifier = ruleIdentifier;
            choicesNodeStack = new Stack<HierarchicalNode>();
            chainNodeStack = new Stack<HierarchicalNode>();

            // There needs to be a starting node that we will allow us to
            // extend upon.
            removedDummyNode = false;
            startingDummyNode = new HierarchicalNode("dummy");
            rootNode = startingDummyNode;
            chainNodeStack.Push(startingDummyNode);
        }

        /// <summary>
        /// Pops an element off of the chain stack and discards it only if
        /// the stack is not empty.
        /// </summary>
        private void PopChainStackIfAny()
        {
            if (chainNodeStack.Count > 0)
            {
                chainNodeStack.Pop();
            }
        }

        /// <summary>
        /// Adds a new word to the compiled structure.
        /// </summary>
        /// <param name="word">The word to add.</param>
        /// <exception cref="ArgumentNullException">If the argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">If the word is an empty string.
        /// </exception>
        public void AddNewWord(string word)
        {
            CheckNotNull(word);
            CheckArgument(word.Length > 0);

            HierarchicalNode node = new HierarchicalNode(word);

            // It either belongs as a child, or it is to be attached to the
            // next node.
            if (addToChoiceAsChild)
            {
                choicesNodeStack.Peek().AddChild(node);
            }
            else
            {
                chainNodeStack.Peek().AddNext(node);
            }

            PopChainStackIfAny(); // Any node on the stack can't be extended from now.
            chainNodeStack.Push(node);
            addToChoiceAsChild = false;
        }

        public void AddNewVariable(HierarchicalNode rootNode)
        {
            AddNewWord("VARIABLE HERE"); // TODO - To be done later
        }

        /// <summary>
        /// Notifies the builder a new choices block is starting.
        /// </summary>
        public void StartNewChoices()
        {
            HierarchicalNode node = new HierarchicalNode("(");

            // If we're nesting this inside another choices node, then it must
            // be a child of that node (or an option). If it's not, then it is
            // a sequence to be added to the current chain.
            if (addToChoiceAsChild)
            {
                choicesNodeStack.Peek().AddChild(node);
            }
            else
            {
                chainNodeStack.Peek().AddNext(node);
            }

            choicesNodeStack.Push(node);
            addToChoiceAsChild = true;

            // It's okay to pop the stack since the one that is on there will
            // not be appended to anymore regardless since either this node
            // goes after it, or this node will appear after it when the EndX()
            // function is called. Therefore the chain will always be able to
            // continue and this node will eventually bubble up to its proper
            // position.
            PopChainStackIfAny();
        }

        public void StartNewOptionalChoices()
        {
            StartNewChoices(); // TODO - Replace me later (this works but isn't correct).
        }

        public void HandlePipeOption()
        {
            PopChainStackIfAny();
            addToChoiceAsChild = true;
        }

        /// <summary>
        /// Signals that a choices block has ended.
        /// </summary>
        public void EndChoices()
        {
            ChoicesOrOptionalEnd();
        }

        /// <summary>
        /// Signals that an optional block has ended.
        /// </summary>
        public void EndOptionalChoices()
        {
            ChoicesOrOptionalEnd();
            chainNodeStack.Peek().SetRange(0, 1); // This node is now on the chain stack.
        }

        /// <summary>
        /// A convenience method for what the ending of a choices and optional
        /// block does.
        /// </summary>
        private void ChoicesOrOptionalEnd()
        {
            Assert(choicesNodeStack.Count > 0);

            PopChainStackIfAny();
            chainNodeStack.Push(choicesNodeStack.Pop());
        }

        /// <summary>
        /// Sets the range of the last node on the chain stack.
        /// </summary>
        /// <param name="lastMinRange">The minimum value. Should not be 
        /// negative.</param>
        /// <param name="lastMaxRange">The maximum value, should be in range:
        /// max(1, lastMinRange).</param>
        public void SetRangeOfMostRecentNode(int lastMinRange, int lastMaxRange)
        {
            chainNodeStack.Peek().SetRange(lastMinRange, lastMaxRange);
        }

        /// <summary>
        /// Gets the root node of what has been added so far.
        /// </summary>
        /// <returns>The root node.</returns>
        public HierarchicalNode GetRootNode()
        {
            if (!removedDummyNode)
            {
                rootNode = rootNode.Next;
                removedDummyNode = true;
            }

            return rootNode;
        }
    }
}
