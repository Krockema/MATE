using Master40.Agents.Agents.Internal;

namespace Master40.Agents.Agents
{
    public class ComunicationAgent : Agent
    {
        public ComunicationAgent(Agent creator, string name, bool debug, string contractType) 
            : base(creator, name, debug)
        {
            ContractType = contractType;
        }
        public string ContractType { get; set; }

        public enum InstuctionsMethods
        {
        }
    }
}