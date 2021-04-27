using System;
using System.Configuration;
using AkkaSim.Logging;
using Hangfire.Console;
using Hangfire.Console.Progress;
using Hangfire.Server;
using Mate.Production.Core.SignalR;
using Mate.Production.Core.SignalR.Messages;
using Microsoft.AspNetCore.SignalR.Client;
using NLog;

namespace Mate.Production.CLI.HangfireConfiguration
{
    public class ProcessingHub : IMessageHub
    {
        private HubConnection _connection;
        private readonly PerformContext _console;
        private IProgressBar _progressBar;
        private readonly Logger _logger;
        private const string HUB_CONNECTION = "HubConnection";
        private const string HUB_CHANNEL = "HubChannel";

        // update value for previously created progress bar
        public ProcessingHub(PerformContext consoleContext)
        {
            _console = consoleContext;
            _logger = LogManager.GetLogger(TargetNames.LOG_AGENTS);
            CreateHubConnection();
        }

        private void CreateHubConnection()
        {
            try
            {
                var hubConnection = ConfigurationManager.AppSettings[HUB_CONNECTION]
                                    + ConfigurationManager.AppSettings[HUB_CHANNEL];
                _connection = new HubConnectionBuilder()
                    .WithUrl(hubConnection)
                    .WithAutomaticReconnect().Build();
                _connection.StartAsync().Wait();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Warn, $"Hub Connection refused, Simulation is executed without MessageHub." 
                                                + $"\r\n Exception: {e.Message}"
                                                + $"\r\n StackTrace: {e.StackTrace}");
                _console.WriteLine("Warning: Hub Connection refused, Simulation is executed without MessageHub. For more Info see Log Files.");
            }
        }

        public void SendToAllClients(string msg, MessageType msgType = MessageType.info)
        {
            _logger.Log(LogLevel.Warn, msg);
        }

        public void SendToClient(string listener, string msg, MessageType msgType = MessageType.info)
        {
            //Do Nothing
        }

        public string ReturnMsgBox(string msg, MessageType type)
        {
            return "";
        }

        public void EndScheduler()
        {
            //Do Nothing
        }

        public void EndSimulation(string msg, string simId, string simNumber)
        {
            Console.Write("\r\n");
            HubConnectionSend("EndSimulation", new[] { msg, simId, simNumber });
            _progressBar.SetValue(100);
            _console.WriteLine($"Simulation Id: {simId} No. {simNumber} Finished.");
            _logger.Log(LogLevel.Info, $"Simulation Id: {simId} No. {simNumber} Finished.");
        }

        public void ProcessingUpdate(int simId, int finished, string simType, int max)
        {
            if (!_logger.IsWarnEnabled)
            {
                Console.Write("\r" + "[Agents][INFO] " + simType);
                _progressBar.SetValue(finished);            
            }
        }        

        public void StartSimulation(string simId, string simNumber)
        {
            HubConnectionSend("StartSimulation", new[] {simId, simNumber});
            //Console.WriteLine($"Simulation Id: {simId} No. {simNumber} started...");
            _logger.Log(LogLevel.Info, $"Simulation Id: {simId} No. {simNumber} started...");
            _console.WriteLine($"Simulation Id: {simId} No. {simNumber} started...");
            _progressBar =  _console.WriteProgressBar();
            
        }

        private void HubConnectionSend(string msg, object[] args)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                _connection.SendCoreAsync(msg, args);
            }
        }

        public void GuardianState(object msg)
        {
            // not required yet
        }
    }
}
