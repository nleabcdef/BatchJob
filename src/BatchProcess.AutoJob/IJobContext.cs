using System.Collections.Generic;
using System.Text;

namespace BatchProcess.AutoJob
{
    public interface IJobContext : IContextStore
    {
        JobId ParentJobId { get; }
        IReadOnlyList<JobId> ProcessedJobs { get; }
        void AddToProcessed(JobId id);
        void PushReportToHookAsync(JobId sender, MessageHook message);
        INotificationManager<JobId> HookManager { get; }
    }
}
