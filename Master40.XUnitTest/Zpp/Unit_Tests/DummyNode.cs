using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Nominal;
using Zpp.Util.Graph;
using Zpp.Util.Graph.impl;

namespace Master40.XUnitTest.Zpp.Unit_Tests
{
    /**
     * DummyNode for unit tests
     */
    public class DummyNode:IScheduleNode
    {
        
        private readonly Id _id;

        public DummyNode(Id id)
        {
            _id = id;
        }
        public DummyNode(int id)
        {
            _id = new Id(id);
        }

        public Id GetId()
        {
            return _id;
        }

        public IScheduleNode GetEntity()
        {
            return this;
        }

        public NodeType GetNodeType()
        {
            return NodeType.Operation;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public DueTime GetEndTimeBackward()
        {
            throw new System.NotImplementedException();
        }

        public DueTime GetStartTimeBackward()
        {
            throw new System.NotImplementedException();
        }

        public void SetStartTimeBackward(DueTime startTime)
        {
            throw new System.NotImplementedException();
        }

        public Duration GetDuration()
        {
            throw new System.NotImplementedException();
        }

        public void SetFinished()
        {
            throw new System.NotImplementedException();
        }

        public void SetInProgress()
        {
            throw new System.NotImplementedException();
        }

        public bool IsFinished()
        {
            throw new System.NotImplementedException();
        }

        public void SetEndTimeBackward(DueTime endTime)
        {
            throw new System.NotImplementedException();
        }

        public void ClearStartTimeBackward()
        {
            throw new System.NotImplementedException();
        }

        public void ClearEndTimeBackward()
        {
            throw new System.NotImplementedException();
        }

        public State? GetState()
        {
            throw new System.NotImplementedException();
        }

        public void SetReadOnly()
        {
            throw new System.NotImplementedException();
        }

        public bool IsReadOnly()
        {
            throw new System.NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            DummyNode other = (DummyNode)obj;
            return _id.Equals(other._id);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}