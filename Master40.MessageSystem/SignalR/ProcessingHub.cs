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

        public void SystemReady2()
        {
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("SignalR Hub active Call Internal.", MessageType.info));
        }
        /*
        public void StartFullPlanning()
        {
            var jobId = BackgroundJob.Enqueue<IProcessMrp>(x =>
                _processMrp.CreateAndProcessOrderDemand(MrpTask.All)
            );
            BackgroundJob.ContinueWith<HubCallback>(jobId, (x => _hubCallback.EndScheduler()));
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("Start full cycle...", MessageType.info));
        }

        public void StartBackwardScheduler()
        {
            var jobId = BackgroundJob.Enqueue<IProcessMrp>(x => 
                _processMrp.CreateAndProcessOrderDemand(MrpTask.Backward)
            );
            BackgroundJob.ContinueWith<HubCallback>(jobId, (x => _hubCallback.EndScheduler()));
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("Start backward...", MessageType.info));
        }

        public void StartForwardScheduler()
        {
            var jobId = BackgroundJob.Enqueue<IProcessMrp>(x => 
                _processMrp.CreateAndProcessOrderDemand(MrpTask.Forward)
            );
            BackgroundJob.ContinueWith<HubCallback>(jobId, (x => _hubCallback.EndScheduler()));
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("Start forward...", MessageType.info));
        }
        public void StartCapacityPlanning()
        {
            var jobId = BackgroundJob.Enqueue<IProcessMrp>(x => 
                _processMrp.CreateAndProcessOrderDemand(MrpTask.Capacity)
            );
            BackgroundJob.ContinueWith<HubCallback>(jobId, (x => _hubCallback.EndScheduler()));
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("Start capacity planning...", MessageType.info));
        }
        
        public void StartGifflerThompsonPlanning()
        {
            var jobId = BackgroundJob.Enqueue<IProcessMrp>(x => 
                _processMrp.CreateAndProcessOrderDemand(MrpTask.GifflerThompson)
            );
            BackgroundJob.ContinueWith<HubCallback>(jobId, (x => _hubCallback.EndScheduler()));
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("Start GT planning...", MessageType.info));
        }
        public void StartSimulate()
        {
            /*
            var jobId = BackgroundJob.Enqueue<ISimulator>(x => _simulator.Simulate());
            BackgroundJob.ContinueWith<HubCallback>(jobId, (x => _hubCallback.EndScheduler()));
            Clients.All.clientListener(_hubCallback.ReturnMsgBox("Start simmulation...", MessageType.info));
            */
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
