using Master40.Agents.Agents.Internal;
using Master40.DB.Migrations;
using Master40.DB.Models;

namespace Master40.Agents.Agents
{
    public class ProductionAgent : Agent
    {
        
        public ProductionAgent(Agent creator, string name, bool debug, ProductionOrder productionOrder) : base(creator, name , debug) { }
    }
}