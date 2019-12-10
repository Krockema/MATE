using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;

namespace Zpp.Util.Graph.impl
{
    public interface IGraphNode: IId
    {
        void AddSuccessor(IGraphNode node);
        
        void AddSuccessors(GraphNodes nodes);
        
        void AddPredecessor(IGraphNode node);
        
        void AddPredecessors(GraphNodes nodes);
        
        GraphNodes GetPredecessors();
        
        GraphNodes GetSuccessors();

        void RemoveSuccessor(IGraphNode node);
        
        void RemovePredecessor(IGraphNode node);
        
        void RemoveAllSuccessors();
        
        void RemoveAllPredecessors();

        INode GetNode();
    }
}