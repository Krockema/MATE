using System;
using AkkaSim.Logging;
using Master40.Tools.Messages;
using Master40.Tools.SignalR;
using NLog;

namespace Master40.Simulation.CLI
{
    public class ConsoleHub: IMessageHub
    {
        Logger _logger = LogManager.GetLogger(TargetNames.LOG_AGENTS);



        public void SendToAllClients(string msg, MessageType msgType)
        {
            _logger.Log(LogLevel.Info, msgType.ToString() + ": " + msg);
            //Console.WriteLine(msg);
        }

        public void SendToClient(string listener, string msg, MessageType msgType)
        {
            _logger.Log(LogLevel.Info, $" {listener}: { msg } ");
        }

        public string ReturnMsgBox(string msg, MessageType type)
        {
            //Console.WriteLine(value: msg);
            return msg;
        }

        public void StartSimulation(string simId, string simNumber)
        {
            _logger.Log(LogLevel.Info, "Start Simulation (id:" + simId + " | simNumber:" + simNumber + ")");
        }

        public void ProcessingUpdate(int simId, int finished, string simType, int max)
        {
            _logger.Log(LogLevel.Info, $"msg");
        }

        public void EndSimulation(string msg, string simId, string simNumber)
        {
            _logger.Log(LogLevel.Info, $"Simulation Id: { simId } | No. { simNumber } Finished.");
        }
    }

}