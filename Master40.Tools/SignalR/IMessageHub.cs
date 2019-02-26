using Master40.DB.Enums;
using Master40.MessageSystem.Messages;

namespace Master40.MessageSystem.SignalR
{
    public interface IMessageHub
    {
        void SendToAllClients(string msg, MessageType msgType = MessageType.info);
        void SendToClient(string listener, string msg, MessageType msgType = MessageType.info);
        string ReturnMsgBox(string msg, MessageType type);
        void EndScheduler();
        void EndSimulation(string msg, string simId, string simNumber);
        void ProcessingUpdate(int simId, int timer, SimulationType simType, int max);
    }
}