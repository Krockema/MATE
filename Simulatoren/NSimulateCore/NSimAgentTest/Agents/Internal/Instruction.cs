using System;
using System.Collections.Generic;
using System.Reflection;

namespace NSimAgentTest.Agents.Internal
{
    public class Instruction
    {
        public string Method { get; set; }
        public Type ExpectedObjecType { get; set; }
    }
}