using Akka.Dispatch.SysMsg;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Helper;
using NLog;

namespace Master40.SimulationCore.Agents.JobAgent.Behaviour
{
    public class Default : Types.Behaviour
    {
        // Tasks of Job Agent
        // Handle Re-queuing
        // Handle Job State - Initial - Waiting - Processing - Finish
        // Sync Resources - Wait for all to send Ready state.
        // Job: Start Operation, Withdraw Material, Bucket
        // Job: Finish Operation, Add Material, Finish Bucket
        // Job: Send / Create Measurements
        // Job: - Dispose

        internal Default(JobConfirmation jobConfirmation, SimulationType simulationType = SimulationType.None)
            : base(childMaker: null, simulationType: simulationType)
        {
            _jobConfirmation = jobConfirmation;
        }

        private JobConfirmation _jobConfirmation { get; set; }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Job.Instruction.TerminateJob msg: Terminate(); break;
                default: return false;
            }
            return true;
        }

        public override bool AfterInit()
        {
            Agent.DebugMessage($"Bucket {_jobConfirmation.Job.Key} is Created!", CustomLogger.JOB, LogLevel.Warn);
            return base.AfterInit();
        }

        private void Terminate()
        {
            Agent.DebugMessage($"Bucket {_jobConfirmation.Job.Key} is Terminated!", CustomLogger.JOB, LogLevel.Warn);
            this.Agent.TryToFinish();
        }
    }
}
