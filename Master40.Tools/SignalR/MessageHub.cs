using System.Collections.Generic;
using System.Linq;
using Master40.MessageSystem.Messages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace Master40.MessageSystem.SignalR
{
    public interface IMessageHub
    {
        void SendToAllClients(string msg);
        void SendToAllClients(string msg, MessageType msgType);
        string ReturnMsgBox(string msg, MessageType type);
        void EndScheduler();
        void EndSimulation(string msg);

        void IncreaseDayCount(int simId);
        int GetDayCount(int simId);

    }

    public class MessageHub : Hub, IMessageHub
    {
        private readonly IConnectionManager _connectionManager;
        private readonly List<SimulationDayCount> _dayCount = new List<SimulationDayCount>();
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
        public void EndSimulation(string text)
        {
            _connectionManager.GetHubContext<ProcessingHub>()
                .Clients.All.clientListener("ProcessingComplete", "text");
        }

        public void IncreaseDayCount(int simId)
        {
            var count = _dayCount.FirstOrDefault(a => a.SimulationId == simId);
            if (count != null)
                _dayCount.Find(a => a.SimulationId == simId).DayCount++;
            else _dayCount.Add(new SimulationDayCount()
                {
                    SimulationId = simId,
                    DayCount = 1
                });
        }

        public int GetDayCount(int simId)
        {
            var count = _dayCount.Where(a => a.SimulationId == simId).ToList();
            return count.Any() ? count.First().DayCount : 0;
        }

    }

    internal class SimulationDayCount
    {
        internal int SimulationId { get; set; }
        internal int DayCount { get; set; }

    }
}