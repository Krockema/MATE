using System;
using Master40.Tools.Messages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace Master40.Tools.SignalR
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
            this._hubContext.Clients.All.SendAsync(method: "clientListener", arg1: ReturnMsgBox(msg: "SignalR Hub active.", type: MessageType.info));
        }

        public void SendToAllClients(string msg, MessageType msgType = MessageType.info)
        {
            this._hubContext.Clients.All.SendAsync(method: "clientListener", arg1: ReturnMsgBox(msg: msg, type: msgType));
        }

        public string ReturnMsgBox(string msg, MessageType type)
        {
            return "<div class=\"alert alert-" + type + "\">" + msg + "</div>";
        }

        public void StartSimulation(string simId, string simNumber)
        {
            this._hubContext.Clients.All.SendAsync(method: "workerListener", arg1:JsonConvert.SerializeObject(value: new[] { "SimulationStart", simId, simNumber } ));
        }

        public void EndSimulation(string text,string simId,string simNumber)
        {
            this._hubContext.Clients.All.SendAsync(method: "workerListener", arg1: JsonConvert.SerializeObject(value: new[] { "SimulationComplete", simId, simNumber } ));
        }

        public void ProcessingUpdate(int simId, int counter, string simType, int max)
        {
            // this._hubContext.Clients.All.SendAsync(method: "clientListener", arg1: "ProcessingUpdate", arg2: simId, arg3: Math.Round(value: (double)counter / max * 100, digits: 0).ToString(), arg4: simType.ToString() );
        }

        public void SendToClient(string listener, string msg, MessageType msgType = MessageType.info)
        {
            this._hubContext.Clients.All.SendAsync(method: listener, arg1: msg);
        }

        public void GuardianState(object msg)
        {
            this._hubContext.Clients.All.SendAsync(method: "clientListener", arg1: JsonConvert.SerializeObject(msg));
        }
    }
}