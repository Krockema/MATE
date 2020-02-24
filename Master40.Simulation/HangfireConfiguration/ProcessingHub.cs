using System;
using System.Configuration;
using AkkaSim.Logging;
using Hangfire.Console;
using Hangfire.Console.Progress;
using Hangfire.Server;
using Master40.Tools.Messages;
using Master40.Tools.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using NLog;

namespace Master40.Simulation.HangfireConfiguration
{
    public class ProcessingHub : IMessageHub
    {
        private readonly HubConnection _connection;
        private readonly PerformContext _console;
        private IProgressBar _progressBar;
        private readonly Logger _logger;
    
        // update value for previously created progress bar
        public ProcessingHub(PerformContext consoleContext)
        {
            _console = consoleContext;
            var hubConnection = ConfigurationManager.AppSettings[index: 3];
            _connection = new HubConnectionBuilder()
                .WithUrl(hubConnection)
                .WithAutomaticReconnect().Build();
            _connection.StartAsync().Wait();
            _logger = LogManager.GetLogger(TargetNames.LOG_AGENTS);
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
            _connection.SendCoreAsync("EndSimulation", new[] { msg, simId, simNumber });
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
            _connection.SendCoreAsync("StartSimulation", new[] { simId, simNumber });
            //Console.WriteLine($"Simulation Id: {simId} No. {simNumber} started...");
            _logger.Log(LogLevel.Info, $"Simulation Id: {simId} No. {simNumber} started...");
            _console.WriteLine($"Simulation Id: {simId} No. {simNumber} started...");
            _progressBar =  _console.WriteProgressBar();
            
        }
    }
}
