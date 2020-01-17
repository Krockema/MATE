using Akka.Util.Internal;
using Hangfire;
using Hangfire.Storage;
using Master40.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.ViewComponents
{
    public class JobInformationViewComponent : ViewComponent
    {
        private readonly IMonitoringApi monitor;
        public JobInformationViewComponent()
        {
            monitor  = JobStorage.Current.GetMonitoringApi();
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var hf = new HangfireJob();
            monitor.EnqueuedJobs("default", 0, int.MaxValue).Select(x => x.Value)
                .ForEach(x => hf.AddJob(x, HangfireJob.QUEUED));
            monitor.FailedJobs(0, int.MaxValue).Select(x => x.Value)
                .ForEach(x => hf.AddJob(x, HangfireJob.FAILED));
            monitor.SucceededJobs(0, int.MaxValue).Select(x => x.Value)
                .ForEach(x => hf.AddJob(x, HangfireJob.SUCCEEDED));
            monitor.ProcessingJobs(0, int.MaxValue).Select(x => x.Value)
                .ForEach(x => hf.AddJob(x, HangfireJob.PROCESSING));

            // foreach (var job in processingJobKeys)
            // {
            //     jobInformations.Add(JobStorage.Current.GetConnection().GetJobData(job));
            // }
             
            return View($"JobInformation", hf);
        }
    }
}