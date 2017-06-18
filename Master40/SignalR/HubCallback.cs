using Master40.Models;
using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace Master40.SignalR
{
    public class HubCallback
    {
        private readonly IConnectionManager _connectionManager;
        public HubCallback(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
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