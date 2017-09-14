using System;
using Master40.MessageSystem.Messages;
using Microsoft.AspNetCore.SignalR;
using Master40.MessageSystem.SignalR;
using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace Master40.MessageSystem.SignalR
{
    public class ProcessingHub : Hub
    {
        /*
        private readonly IProcessMrp _processMrp;
        */
        private readonly IMessageHub _hubCallback;

        public ProcessingHub(IMessageHub hubCallback) //, IProcessMrp processMrp)
        {
            //_processMrp = processMrp;
            _hubCallback = hubCallback;
        }
        
        public void SystemReady()
        {
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("SignalR Hub active.", MessageType.info));
        }

        public void SignalReady()
        {
            Clients.All.clientListener("SignalReady");
        }


        public void SystemReady2()
        {
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("SignalR Hub active Call Internal.", MessageType.info));
        }
    }
}

    //Works
    public class HangfireTest
    {
        private readonly IConnectionManager _connectionManager;

        public HangfireTest(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }
        public void Test(int d)
        {
            Console.WriteLine("BackgroundJob Execution");
            _connectionManager.GetHubContext<ProcessingHub>()
                .Clients.All.clientListener("Forward has Finished.");
        }
    }
