using Master40.DB.Data.Helper;
using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace Master40.MessageSystem.SignalR
{
    public class MessageHub
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
                .Clients.All.clientListener("MrpProcessingComplete");
        }

    }
}