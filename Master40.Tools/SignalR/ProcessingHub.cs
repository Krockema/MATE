using Master40.Tools.Messages;
using Microsoft.AspNetCore.SignalR.Client;

namespace Master40.Tools.SignalR
{
    public class ProcessingHub : IMessageHub
    {
        private readonly HubConnection _connection;
        public ProcessingHub()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://141.56.137.26:5000/MessageHub")
                .WithAutomaticReconnect().Build();
            _connection.StartAsync().Wait();
        }

        public void SendToAllClients(string msg, MessageType msgType = MessageType.info)
        {
            //throw new System.NotImplementedException();
        }

        public void SendToClient(string listener, string msg, MessageType msgType = MessageType.info)
        {
            //throw new System.NotImplementedException();
        }

        public string ReturnMsgBox(string msg, MessageType type)
        {
            return "";
        }

        public void EndScheduler()
        {
            //throw new System.NotImplementedException();
        }

        public void EndSimulation(string msg, string simId, string simNumber)
        {
            _connection.SendCoreAsync("EndSimulation", new[] { msg, simId, simNumber });
        }

        public void ProcessingUpdate(int simId, int finished, string simType, int max)
        {
            //throw new System.NotImplementedException();
        }

        public void StartSimulation(string simId, string simNumber)
        {
            _connection.SendCoreAsync("StartSimulation", new[] { simId, simNumber });
        }
    }
}
