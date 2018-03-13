using BatchProcess.Shared;

namespace BatchProcess.AutoJob.Runtime
{
    /// <summary>
    /// process the workflow jobs in parallel fashion
    /// </summary>
    public partial class TaskRunner : IWorkflowHost<TaskRuntime>, IWorkflowRunner
    {
        private void StartWorkflow()
        {
            if (_workflow.Count == 0)
            {
                _result = new JobResult(
                    JobStatus.CompletedWithError,
                    new AutoJobException(Current.Id, null, _msgNoWorkflow));
                _jobStatus[Current.Id.ToString()] = _result;
                return;
            }
            if (mLock.WaitOne())
            {
                try
                {
                    IJobContext context = null;
                    if (Current.Option == ShareContext.Parent)
                        context = Current.Context;
                    else if (Current.Option == ShareContext.First || Current.Option == ShareContext.previous)
                        context = _workflow[0].Context;

                    _workflow.ForEach(w =>
                    {
                        w.Context = context;
                        _workflowThread.AddAndStart(() => Run(w), w.Id.ToString(), ref _cancelToken, true);
                    });
                }
                finally
                {
                    mLock.ReleaseMutex();
                }
            }
        }

        private JobResult Run(IAutomatedJob job)
        {
            JobResult result = null;

            result = ErrorHandle.Expect(() => job.Doable(),
                out bool anyError,
                string.Format(_msgWError, Current.Id.Id, Current.Id.Name),
                string.Format(_msgJobError, job.Id.Id, job.Id.Name));

            if (anyError)
                result = new JobResult(JobStatus.CompletedWithError, new AutoJobException(job.Id, null, _msgError));

            _jobStatus[job.Id.ToString()] = result;
            return result;
        }
    }
}
