using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.XUnitTest.Preparations
{
    public class Tree
    {
        public object[,] accessor;

    }



    public class Node {
        private int LastIndex = 0;
        public bool Active = false;
        public string Name { get; set; }
        public Gateway Gateway { get; set; }
        public NodeValue[] NodeValues { get; set; }
        public void Advance() {
            Active = true;
            LastIndex++;
        }
        public NodeValue GetCurrentValue => NodeValues[LastIndex];
    }
    public abstract class Gateway {
        protected int LastParameterIndex = 0;
        protected int LastNodeIndex = 0;
        public Node[][] Nodes { get; set; }
        public abstract void NodeValues(List<NodeValue> prunedValues);

    }
    public class And : Gateway
    {
        public And()
        {
            Nodes = new Node[1][];
            Nodes[0] = new Node[] { new Node(), new Node() };
        }
        public override void NodeValues(List<NodeValue> prunedValues)
        {
            throw new NotImplementedException();
        }
        public void Advance()
        {
            if (LastParameterIndex == Nodes.Length)
            {
                LastParameterIndex = 0;
                if (LastNodeIndex == Nodes[LastParameterIndex].Length)
                {
                    LastNodeIndex = 0;
                }
                else
                {
                    LastNodeIndex++;
                }
            }
            else
            {
                LastParameterIndex++;
            }
        }

    }
    public class Or : Gateway
    {
        public override void NodeValues(List<NodeValue> prunedValues)
        {
            
            foreach (var item in Nodes)
            {
                foreach (var item2 in item)
                {
                    if(item2.Active) prunedValues.Add(item: item2.GetCurrentValue);
                }
            }
        }
        public bool Advance()
        {
            if (LastParameterIndex == Nodes.Length)
            {
                if (LastNodeIndex != Nodes[LastParameterIndex].Length)
                {
                    LastNodeIndex++;
                    return true;
                }
            }
            else
            {
                LastParameterIndex++;
                return true;
            }
            // else finished with this node
            return false;
        }
    }

    public class Const : Gateway
    {
        public override void NodeValues(List<NodeValue> prunedValues)
        {
            prunedValues.AddRange(collection: Nodes[0][0].NodeValues);
        }
    }

    public class NodeValue
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public object Item { get; set; }
    }
}
