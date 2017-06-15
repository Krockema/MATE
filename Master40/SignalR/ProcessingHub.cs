using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Master40.BusinessLogic.MRP;
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
            Clients.All.clientListener("SignalR Hub active.");
        }

        public void StartBackwardScheduler()
        {
            var jobId = BackgroundJob.Enqueue<IProcessMrp>(x => //ht.Test(1)
                _processMrp.CreateAndProcessOrderDemand(MrpTask.Backward)
            );
            Clients.All.clientListener("Backward Has Been Started.");
        }

        public void EndBackwardScheduler()
        {
            Clients.All.clientListener("Backward Has Been Finished.");
        }
    }


    //Works
    public class HangfireTest
    {
        public void Test(int d)
        {
            Console.WriteLine("BackgroundJob Execution");
        }
    }
}