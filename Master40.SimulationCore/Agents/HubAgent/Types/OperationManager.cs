using System;
using System.Collections.Generic;
using System.Linq;
using static FOperations;
using static FUpdateStartConditions;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class OperationManager : HashSet<JobConfirmation>
    {
        internal IJob GetJobBy(Guid operationKey)
        {
            return this.SingleOrDefault(x => x.Job.Key == operationKey)?.Job;
        }

        internal JobConfirmation UpdateOperationStartConfirmation(FUpdateStartCondition startCondition)
        {
            var jobConfirmation = GetJobConfirmation(startCondition.OperationKey);
            var operation = jobConfirmation.Job as FOperation;
            operation.SetStartConditions(startCondition.PreCondition, startCondition.ArticlesProvided);
            return jobConfirmation;
        }

        internal JobConfirmation GetJobConfirmation(Guid operationKey)
        {
            return this.SingleOrDefault(x => x.Job.Key == operationKey);
        }

        internal void Reset(Guid key)
        {
            throw new NotImplementedException();
        }
    }
}