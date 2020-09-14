using Master40.DB.GanttPlanModel;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Nominal;
using System;
using Master40.DB.Data.Context;
using Master40.Tools.ExtensionMethods;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Central
{
    public class ConfirmationManager
    {
        private string _dbConnectionStringGanttPlan { get; }
        public List<GptblConfirmation> _confirmations { get; private set; } = new List<GptblConfirmation>();
        public List<GptblConfirmationResource> _confirmationsResources { get; private set; } = new List<GptblConfirmationResource>();

        public ConfirmationManager(string dbConnectionStringGanttPlan)
        {
            _dbConnectionStringGanttPlan = dbConnectionStringGanttPlan;
        }

        public void TransferConfirmations()
        {
            using (var localGanttPlanDbContext = GanttPlanDBContext.GetContext(_dbConnectionStringGanttPlan))
            {
                localGanttPlanDbContext.GptblConfirmation.AddRange(_confirmations);
                localGanttPlanDbContext.GptblConfirmationResource.AddRange(_confirmationsResources);
                localGanttPlanDbContext.SaveChanges();
                
            }

            ResetConfirmations();
        }

        public void ResetConfirmations()
        {
            _confirmations.Clear();
            _confirmationsResources.Clear();
        }

        public void AddConfirmations(GptblProductionorderOperationActivity activity, GanttState confirmationType)
        {
            AddConfirmation(activity, confirmationType);
            // only finish !?
            if (confirmationType.NotEqual(GanttState.Finished))
                return;
            
            foreach (var resource in activity.ProductionorderOperationActivityResources)
            {
                AddResourceConfirmation(resource);
            }
        }

        public void AddConfirmation(GptblProductionorderOperationActivity activity, GanttState confirmationType)
        {
            var confirmation = new GptblConfirmation();

            confirmation.ClientId = string.Empty;
            confirmation.ConfirmationId = Guid.NewGuid().ToString();
            confirmation.Info1 = string.Empty;
            confirmation.Info2 = string.Empty;
            confirmation.Info3 = string.Empty;
            confirmation.Name = activity.Name;
            confirmation.ActivityEnd = confirmationType == GanttState.Finished ? activity.DateEnd : null; 
            confirmation.ActivityStart = activity.DateStart;
            confirmation.ConfirmationType = (int)confirmationType;
            confirmation.ConfirmationDate = activity.DateStart;
            confirmation.ProductionorderActivityId = activity.ActivityId;
            confirmation.ProductionorderId = activity.ProductionorderId;
            confirmation.ProductionorderOperationId = activity.OperationId;
            confirmation.ProductionorderSplitId = 0;
            confirmation.ProductionorderAlternativeId = string.Empty;
            confirmation.QuantityFinished = confirmationType == GanttState.Finished ? 100 : 0;
            confirmation.QuantityFinishedUnitId = "%";
            confirmation.LastModified = activity.DateStart;

            _confirmations.Add(confirmation);
        }

        public void AddResourceConfirmation(GptblProductionorderOperationActivityResource resource)
        {
            var confirmationResource = new GptblConfirmationResource();
            confirmationResource.ClientId = string.Empty;
            confirmationResource.ConfirmationId = new Guid().ToString();
            confirmationResource.ResourceId = resource.ResourceId;
            confirmationResource.ResourceType = resource.ResourceType;
            confirmationResource.GroupId = resource.GroupId;
            confirmationResource.Allocation = 100;

        _confirmationsResources.Add(confirmationResource);
        }

    }
}
