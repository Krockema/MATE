using Master40.MessageSystem.Messages;
using Microsoft.AspNetCore.SignalR;
using Master40.DB.Enums;
using System;

namespace Master40.MessageSystem.SignalR
{
    public interface IMessageHub
    {
        void SendToAllClients(string msg, MessageType msgType = MessageType.info);
        string ReturnMsgBox(string msg, MessageType type);
        void EndScheduler();
        void EndSimulation(string msg, string simId, string simNumber);
        void ProcessingUpdate(int simId, int timer, SimulationType simType, int max);
    }

    public class MessageHub : Hub, IMessageHub
    {
        private readonly IHubContext<MessageHub> _connectionManager;
        public MessageHub(IHubContext<MessageHub> connectionManager)
        {
            _connectionManager = connectionManager;
        }


        public void SendToAllClients(string msg, MessageType msgType = MessageType.info)
        {
            _connectionManager
                .Clients.All.SendAsync("clientListener", ReturnMsgBox(msg, msgType));
        }

        public string ReturnMsgBox(string msg, MessageType type)
        {
            return "<div class=\"alert alert-" + type + "\">" + msg + "</div>";
        }
        public void EndScheduler()
        {
            _connectionManager
                .Clients.All.SendAsync("clientListener", "MrpProcessingComplete", 1);
        }
        public void EndSimulation(string text,string simId,string simNumber)
        {
            _connectionManager
                .Clients.All.SendAsync("clientListener", "ProcessingComplete", simId, simNumber);
        }

        public void ProcessingUpdate(int simId, int counter, SimulationType simType, int max)
        {
            _connectionManager
               .Clients.All.SendAsync("clientListener", "ProcessingUpdate", simId, Math.Round((double)counter / max * 100, 0).ToString(), simType.ToString() );
        }
    }
}