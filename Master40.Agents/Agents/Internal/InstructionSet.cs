using System;

namespace Master40.Agents.Agents.Internal
{
    public class InstructionSet
    {
        public InstructionSet()
        {
            Id = Guid.NewGuid();
            WaitFor = 0;
        }
        public Guid Id { get; }
        public string MethodName { get; set; }
        public Agent SourceAgent { get; set; }
        // Maybe Works with Dynamic as well Not shure yet.
        public object ObjectToProcess { get; set; }
        public Type ObjectType { get; set; }
        public long WaitFor { get; set; }
    }

    
}