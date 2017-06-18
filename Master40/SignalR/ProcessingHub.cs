using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Master40.BusinessLogic.MRP;
using Master40.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Master40.BusinessLogic.Simulation;

namespace Master40.SignalR
{
    public class ProcessingHub : Hub
    {
        private readonly IProcessMrp _processMrp;
        private readonly ISimulator _simulator;
        private readonly HubCallback _hubCallback;

        public ProcessingHub(IProcessMrp processMrp, ISimulator simulator, HubCallback hubCallback)
        {
            _processMrp = processMrp;
            _simulator = simulator;
            _hubCallback = hubCallback;
        }
        public void SystemReady()
        {
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("SignalR Hub active.", MessageType.info));
        }
        
        public void StartFullPlanning()
        {
            var jobId = BackgroundJob.Enqueue<IProcessMrp>(x =>
                _processMrp.CreateAndProcessOrderDemand(MrpTask.All)
            );
            BackgroundJob.ContinueWith<HubCallback>(jobId, (x => _hubCallback.EndScheduler()));
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("Start Full Cycle...", MessageType.info));
        }

        public void StartBackwardScheduler()
        {
            var jobId = BackgroundJob.Enqueue<IProcessMrp>(x => 
                _processMrp.CreateAndProcessOrderDemand(MrpTask.Backward)
            );
            BackgroundJob.ContinueWith<HubCallback>(jobId, (x => _hubCallback.EndScheduler()));
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("Start Backward...", MessageType.info));
        }

        public void StartForwardScheduler()
        {
            var jobId = BackgroundJob.Enqueue<IProcessMrp>(x => 
                _processMrp.CreateAndProcessOrderDemand(MrpTask.Forward)
            );
            BackgroundJob.ContinueWith<HubCallback>(jobId, (x => _hubCallback.EndScheduler()));
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("Start Forwart...", MessageType.info));
        }
        public void StartCapacityPlanning()
        {
            var jobId = BackgroundJob.Enqueue<IProcessMrp>(x => 
                _processMrp.CreateAndProcessOrderDemand(MrpTask.Capacity)
            );
            BackgroundJob.ContinueWith<HubCallback>(jobId, (x => _hubCallback.EndScheduler()));
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("Start Capacity Planning...", MessageType.info));
        }
        
        public void StartGifflerThompsonPlanning()
        {
            var jobId = BackgroundJob.Enqueue<IProcessMrp>(x => 
                _processMrp.CreateAndProcessOrderDemand(MrpTask.GifflerThompson)
            );
            BackgroundJob.ContinueWith<HubCallback>(jobId, (x => _hubCallback.EndScheduler()));
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("Start GT Planning...", MessageType.info));
        }
        public void StartSimulate()
        {
            var jobId = BackgroundJob.Enqueue<ISimulator>(x => _simulator.Simulate());
            BackgroundJob.ContinueWith<HubCallback>(jobId, (x => _hubCallback.EndScheduler()));
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("Start Simmulation...", MessageType.info));
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