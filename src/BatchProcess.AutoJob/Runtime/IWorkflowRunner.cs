using System;
using System.Threading;

namespace BatchProcess.AutoJob.Runtime
{
    /// <summary>
    /// Job execution controller/manager used to control runtime execution using stop, wait, status methods
    /// </summary>
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
