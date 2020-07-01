using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;
using Zpp.Util.Graph.impl;

namespace Zpp.Util.Graph
{
    public interface INode: IId
    {
    
        NodeType GetNodeType();

        IScheduleNode GetEntity();
    }
}