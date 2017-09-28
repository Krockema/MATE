using System.Collections.Generic;
using System.Linq;
using Master40.MessageSystem.Messages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Master40.DB.Enums;
using System;

namespace Master40.MessageSystem.SignalR
{
    public interface IMessageHub
    {
        void SendToAllClients(string msg);
        void SendToAllClients(string msg, MessageType msgType);
        string ReturnMsgBox(string msg, MessageType type);
        void EndScheduler();
        void EndSimulation(string msg);
        void ProcessingUpdate(int simId, int timer, SimulationType simType, int max);
    }

    public class MessageHub : Hub, IMessageHub
    {
        private readonly IConnectionManager _connectionManager;
        public MessageHub(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public void SendToAllClients(string msg)
        {
            _connectionManager.GetHubContext<ProcessingHub>()
                .Clients.All.clientListener(ReturnMsgBox(msg, MessageType.info));
        }
        public void SendToAllClients(string msg, MessageType msgType)
        {
            _connectionManager.GetHubContext<ProcessingHub>()
                .Clients.All.clientListener(ReturnMsgBox(msg, msgType));
        }

        public string ReturnMsgBox(string msg, MessageType type)
        {
            return "<div class=\"alert alert-" + type + "\">" + msg + "</div>";
        }
        public void EndScheduler()
        {
            _connectionManager.GetHubContext<ProcessingHub>()
                .Clients.All.clientListener("MrpProcessingComplete", 1);
        }
        public void EndSimulation(string text)
        {
            _connectionManager.GetHubContext<ProcessingHub>()
                .Clients.All.clientListener("ProcessingComplete", 1);
        }

        public void ProcessingUpdate(int simId, int counter, SimulationType simType, int max)
        {
            _connectionManager.GetHubContext<ProcessingHub>()
               .Clients.All.clientListener("ProcessingUpdate", simId, Math.Round((double)counter / max * 100, 0).ToString(), simType.ToString() );
        }
    }
}