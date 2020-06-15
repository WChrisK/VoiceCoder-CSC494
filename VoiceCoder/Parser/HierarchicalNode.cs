//  A node that can be stored in a hierarchial list.
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
using static VoiceCoder.Util.Assertion;

namespace VoiceCoder.Parser
{
    public class HierarchicalNode
    {
        public enum TokenType
        {
            Single = 0, // 0
            Options,    // 1
            Freestyle   // 2
        }

        public HierarchicalNode Next { get; private set; }

        public IReadOnlyList<HierarchicalNode> Children { get { return _Children.AsReadOnly(); } }
        private List<HierarchicalNode> _Children;

        public int MinRepeat { get; private set; }

        public int MaxRepeat { get; private set; }

        public string Value { get; }

        public HierarchicalNode(string value)
        {
            CheckNotNull(value);
            CheckArgument(value.Length > 0);

            _Children = new List<HierarchicalNode>();
            MinRepeat = 1;
            MaxRepeat = 1;
            Value = value;
        }

        public void AddNext(HierarchicalNode node)
        {
            CheckNotNull(node);

            Next = node;
        }

        public void AddChild(HierarchicalNode node)
        {
            CheckNotNull(node);

            _Children.Add(node);
        }

        public void SetRange(int min, int max)
        {
            CheckArgument(0 <= min);
            CheckArgument(min <= max);
            CheckArgument(max != 0);

            MinRepeat = min;
            MaxRepeat = max;
        }

        /// <summary>
        /// Assists the ToString method by getting the repeatable value.
        /// </summary>
        /// <returns></returns>
        private string GetStringRepeatableRangeIfPresent()
        {
            if (MinRepeat == 1 && MaxRepeat == 1)
            {
                return "";
            }
            else if (MinRepeat == 0 && MaxRepeat == 1)
            {
                return ""; // It will be represented by square brackets, so it's implied.
            }
            else if (MaxRepeat == int.MaxValue)
            {
                if (MinRepeat == 0)
                {
                    return "*";
                }
                else if (MinRepeat == 1)
                {
                    return "+";
                }

                return "{" + MinRepeat + ",}";
            }
            else if (MinRepeat == MaxRepeat)
            {
                return "{" + MinRepeat + "}";
            }
            return "{" + MinRepeat + "," + MaxRepeat + "}";
        }

        /// <summary>
        /// Recursively builds a string representation for this node that can
        /// be placed in a grammar file.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string output = "";

            if (_Children.Count > 0)
            {
                bool isOptional = (MinRepeat == 0 && MaxRepeat == 1);
                output += isOptional ? "[" : "(";
                for (int i = 0; i < _Children.Count; i++)
                {
                    output += _Children[i].ToString();
                    output += (i < _Children.Count - 1 ? " | " : "");
                }
                output += isOptional ? "]" : ")";
            }
            else
            {
                output += Value;
            }

            output += GetStringRepeatableRangeIfPresent();

            if (Next != null)
            {
                output += " " + Next.ToString();
            }

            return output;
        }
    }
}
