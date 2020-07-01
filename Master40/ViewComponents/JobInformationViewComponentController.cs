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
        private readonly IMonitoringApi _monitor;
        private readonly HangfireJob _jobCache;
        public JobInformationViewComponent(HangfireJob hangfireJobs)
        {
            _monitor  = JobStorage.Current.GetMonitoringApi();
            _jobCache = hangfireJobs;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cache = await Task.Run(CreateJobCache);
            return View($"JobInformation", cache);
        }

        private HangfireJob CreateJobCache()
        {
            if (_jobCache.HasJobs)
                return _jobCache;
            //else

            _monitor.EnqueuedJobs("default", 0, int.MaxValue).Select(x => x.Value)
                .ForEach(x => _jobCache.AddJob(x, HangfireJob.QUEUED));
            _monitor.ScheduledJobs(0, int.MaxValue).Select(x => x.Value)
                .ForEach(x => _jobCache.AddJob(x, HangfireJob.QUEUED));
            _monitor.FailedJobs(0, int.MaxValue).Select(x => x.Value)
                .ForEach(x => _jobCache.AddJob(x, HangfireJob.FAILED));
            _monitor.SucceededJobs(0, int.MaxValue).Select(x => x.Value)
                .ForEach(x => _jobCache.AddJob(x, HangfireJob.SUCCEEDED));
            _monitor.ProcessingJobs(0, int.MaxValue).Select(x => x.Value)
                .ForEach(x => _jobCache.AddJob(x, HangfireJob.PROCESSING));
            return _jobCache;
        }
    }
}