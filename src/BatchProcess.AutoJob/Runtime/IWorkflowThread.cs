using System;
using System.Text;
using System.Threading;

namespace BatchProcess.AutoJob.Runtime
{
    /// <summary>
    /// Threading infra required by job exection runtime.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
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
