using Akka.Util.Internal;
using Hangfire;
using Hangfire.Storage;
using Master40.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Akka.Dispatch.SysMsg;

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
            CreateJobCache();
            return View($"JobInformation", _jobCache);
        }

        private void CreateJobCache()
        {
            if (_jobCache.HasJobs)
                return;
            //else

            _monitor.EnqueuedJobs("default", 0, int.MaxValue).Select(x => x.Value)
                .ForEach(x => _jobCache.AddJob(x, HangfireJob.QUEUED));
            _monitor.FailedJobs(0, int.MaxValue).Select(x => x.Value)
                .ForEach(x => _jobCache.AddJob(x, HangfireJob.FAILED));
            _monitor.SucceededJobs(0, int.MaxValue).Select(x => x.Value)
                .ForEach(x => _jobCache.AddJob(x, HangfireJob.SUCCEEDED));
            _monitor.ProcessingJobs(0, int.MaxValue).Select(x => x.Value)
                .ForEach(x => _jobCache.AddJob(x, HangfireJob.PROCESSING));
        }
    }
}