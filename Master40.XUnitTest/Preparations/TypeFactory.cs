using Akka.Actor;
using Master40.DB.DataModel;
using Master40.SimulationCore.Helper;
using static FOperations;

namespace Master40.XUnitTest.Preparations
{
    public static class TypeFactory
    {
        public static FOperation CreateJobItem(string jobName, int jobDuration, bool preCondition = true, int dueTime = 50, string skillName = "Sewing")
        {
            var operation = new M_Operation()
            {
                ArticleId = 10,
                AverageTransitionDuration = 20,
                Duration = jobDuration,
                HierarchyNumber = 10,
                Id = 1,
                Name = jobName,
                ResourceSkill = new M_ResourceSkill() { Name = skillName }
            };
            return operation.ToOperationItem(dueTime: 50, productionAgent: ActorRefs.Nobody, firstOperation: preCondition, currentTime: 0);
        }
    }
}