using Master40.DB.Nominal;
using System;

namespace Master40.SimulationCore.Agents.StorageAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
    {

        public Central(SimulationType simType) : base(simulationType: simType)
        {
        }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Storage.Instruction.RequestArticle msg: RequestArticle(requestItem: msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void RequestArticle(FArticles.FArticle requestItem)
        {
            throw new NotImplementedException();
        }
    }
}
