using System;
using System.Text;
using System.Threading;

namespace BatchProcess.AutoJob.Runtime
{
    public interface IWorkflowThread<TResult>
        where TResult : class
    {
        void StopAll();
        void AddAndStart(Func<TResult> task, string id, ref CancellationToken token, bool addCancelToken = false);
        TResult GetResult(string id);
        JobStatus GetStatus(string id);
        JobStatus GetStatus();
        bool WaitForAll();
    }
}
