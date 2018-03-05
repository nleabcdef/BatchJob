using System;
using System.Threading;

namespace BatchProcess.AutoJob.Runtime
{
    public interface IWorkflowRunner
    {
        void SoftStop();
        JobStatus GetStatus(JobId workflowId);
        Mutex mLock { get; }
        IWorkflowJob Current { get; }
        JobResult GetResult(JobId workflowId);
        bool WaitForAll();
    }
}
