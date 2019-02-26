using Master40.MessageSystem.Messages;
using Microsoft.AspNetCore.SignalR;
using Master40.DB.Enums;
using System;

namespace Master40.MessageSystem.SignalR
{
    public class MessageHub : Hub, IMessageHub
    {
        protected IHubContext<MessageHub> _hubContext;
        public MessageHub(IHubContext<MessageHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public void SystemReady()
        {
            this._hubContext.Clients.All.SendAsync("clientListener", ReturnMsgBox("SignalR Hub active.", MessageType.info));
        }

        public void SendToAllClients(string msg, MessageType msgType = MessageType.info)
        {
            this._hubContext.Clients.All.SendAsync("clientListener", ReturnMsgBox(msg, msgType));
        }

        public string ReturnMsgBox(string msg, MessageType type)
        {
            return "<div class=\"alert alert-" + type + "\">" + msg + "</div>";
        }
        public void EndScheduler()
        {
            this._hubContext.Clients.All.SendAsync("clientListener", "MrpProcessingComplete", 1);
        }
        public void EndSimulation(string text,string simId,string simNumber)
        {
            this._hubContext.Clients.All.SendAsync("clientListener", "ProcessingComplete", simId, simNumber);
        }

        public void ProcessingUpdate(int simId, int counter, SimulationType simType, int max)
        {
            this._hubContext.Clients.All.SendAsync("clientListener", "ProcessingUpdate", simId, Math.Round((double)counter / max * 100, 0).ToString(), simType.ToString() );
        }

        public void SendToClient(string listener, string msg, MessageType msgType = MessageType.info)
        {
            this._hubContext.Clients.All.SendAsync(listener, msg);
        }
    }
}