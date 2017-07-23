using System;

namespace NSimAgentTest.Agents.Internal
{
    public class InstructionSet
    {
        public InstructionSet()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; }
        public string MethodName { get; set; }
        public Agent SourceAgent { get; set; }
        // Maybe Works with Dynamic as well Not shure yet.
        public object ObjectToProcess { get; set; }
        public Type ObjectType { get; set; }
    }

    
}