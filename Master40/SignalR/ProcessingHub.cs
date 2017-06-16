using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Master40.BusinessLogic.MRP;
using Master40.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace Master40.SignalR
{
    public class ProcessingHub : Hub
    {
        private readonly IProcessMrp _processMrp;
        public ProcessingHub(IProcessMrp processMrp)
        {
            _processMrp = processMrp;
        }
        public void SystemReady()
        {
            Clients.All.clientListener(Callback.ReturnMsgBox("SignalR Hub active.", MessageType.info));
        }

        public void StartBackwardScheduler()
        {
            var jobId = BackgroundJob.Enqueue<IProcessMrp>(x => //ht.Test(1)
                _processMrp.CreateAndProcessOrderDemand(MrpTask.Backward)
            );
            BackgroundJob.ContinueWith<IProcessMrp>(jobId, (x => _processMrp.EndBackwardScheduler()));
            Clients.All.clientListener(Callback.ReturnMsgBox("Start Backward...", MessageType.info));
        }

        public void EndBackwardScheduler()
        {
            Clients.All.clientListener(Callback.ReturnMsgBox("Finished Backward!", MessageType.info));
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
}