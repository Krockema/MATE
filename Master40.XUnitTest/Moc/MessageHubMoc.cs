using Master40.DB.Enums;
using Master40.MessageSystem.Messages;
using Master40.MessageSystem.SignalR;
using System.Diagnostics;

namespace Master40.XUnitTest.Moc
{
    public class MessageHub : IMessageHub
    {
        public void SendToAllClients(string msg)
        {
            Debug.WriteLine(msg);
        }

        public void SendToAllClients(string msg, MessageType msgType)
        {
            Debug.WriteLine(msg);
        }

        public void SendToClient(string listener, string msg, MessageType msgType)
        {
            Debug.WriteLine(msg, listener);
        }

        public string ReturnMsgBox(string msg, MessageType type)
        {
            Debug.WriteLine(msg);
            return msg;
        }

        public void EndScheduler()
        {
            Debug.WriteLine("Finished Scheduler");
        }

        public void EndSimulation(string msg)
        {
            Debug.WriteLine(msg);
        }

        public void ProcessingUpdate(int simId, int timer, SimulationType simType, int max)
        {
            //Do nothing 
        }

        public void EndSimulation(string msg, string simId, string simNumber)
        {
            throw new System.NotImplementedException();
        }
    }

}