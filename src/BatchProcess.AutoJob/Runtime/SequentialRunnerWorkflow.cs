using BatchProcess.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace BatchProcess.AutoJob.Runtime
{
    /// <summary>
    /// process the workflow jobs in sequential fashion
    /// </summary>
    public partial class SequentialRunner : IWorkflowHost<SequentialRuntime>, IWorkflowRunner
    {
        private JobResult StartWorkflow()
        {
            if (_workflow.Count == 0)
                return new JobResult(
                    JobStatus.CompletedWithError,
                    new AutoJobException(Current.Id, null, _msgNoWorkflow));

            if (!mLock.WaitOne())
                return null;

            JobResult result = null;
            var jobQueue = new Queue<IAutomatedJob>();
            _workflow.ForEach(j => { jobQueue.Enqueue(j); });
            IJobContext context = null;

            context = GetJobContext();
            result = ProcessJobs(jobQueue, context);

            mLock.ReleaseMutex();
            return result;
        }

        private IJobContext GetJobContext()
        {
            if (Current.Option == ShareContext.Parent)
                return Current.Context;

            if (Current.Option == ShareContext.First || Current.Option == ShareContext.previous)
                return _workflow[0].Context;

            return null;
        }

        private JobResult ProcessJob(IAutomatedJob job, IJobContext context, out bool anyError)
        {
            JobResult innerResult = null;
            anyError = false;

            if (!job.mLock.WaitOne())
                return new JobResult(
                    JobStatus.CompletedWithError,
                    new AutoJobException(Current.Id,
                    null,
                    string.Format(_msgAbortError,
                        job.Id.Id, job.Id.Name)));

            innerResult = ProcessWithHooks(job, context, out anyError);
            job.mLock.ReleaseMutex();

            return innerResult;
        }

        private JobResult ProcessWithHooks(IAutomatedJob job, IJobContext context, out bool anyError)
        {
            JobResult innerResult = null;
            var jId = job.Id.ToString();
            var wId = Current.Id.ToString();
            anyError = false;
            job.Context = context;
            job.Context.PushReportToHookAsync(job.Id, 
                new MessageHook(string.Format(_msgJobSatrted, jId, wId), 
                    MessageType.Info.ToString(), MessageType.Info));

            try
            {
                innerResult = job.Doable();
            }
            catch(Exception ex)
            {
                anyError = true;
                var error = string.Join(Environment.NewLine,
                    string.Format(_msgWError, Current.Id.Id, Current.Id.Name),
                    string.Format(_msgJobError, job.Id.Id, job.Id.Name),
                    ex.Message, ex.StackTrace);

                job.Context.PushReportToHookAsync(job.Id,
                    new MessageHook(error,
                    MessageType.Error.ToString(), MessageType.Error));
                ErrorHandle.Logger.Log(ex);
            }
            finally
            {
                job.Context.PushReportToHookAsync(job.Id,
                    new MessageHook(string.Format(_msgJobCompleted, jId, wId),
                    MessageType.Info.ToString(), MessageType.Info));
            }

            return innerResult;
        }

        private JobResult ProcessWorkflow(IAutomatedJob job, IJobContext context, out bool anyError)
        {
            anyError = false;
            return ProcessWithHooks(job, context, out anyError);
        }

        private JobResult ProcessJobs(Queue<IAutomatedJob> jobs, IJobContext context)
        {
            JobResult result = null;
            JobResult innerResult = null;
            var job = jobs.Dequeue();
            while (job != null)
            {
                innerResult = null;
                _currentJobId = job.Id;

                innerResult = job is IWorkflowJob ?
                    ProcessWorkflow(job, context, out bool anyError) :
                    ProcessJob(job, context, out anyError);

                _jobStatus[job.Id.ToString()] = anyError ?
                                JobStatus.CompletedWithError
                                : innerResult.Status;

                _currentJobId = null;

                var canStop = Current.OnFailure == WhenFailure.StopOrExitJob &&
                    (innerResult == null || innerResult.Status != JobStatus.Completed);

                if (canStop)
                    return new JobResult(JobStatus.CompletedWithError,
                        new AutoJobException(Current.Id,
                        innerResult?.Error, _msgAborted));

                context = Current.Option == ShareContext.previous ? job.Context : context;

                if (jobs.Count == 0)
                    break;

                if (CheckForCancellation(ref innerResult))
                    break;

                job = jobs.Dequeue();
            }

            result = innerResult == null ?
                new JobResult(JobStatus.Completed)
                : new JobResult(innerResult.Status, innerResult.Error);

            return result;
        }

        private bool CheckForCancellation(ref JobResult result)
        {
            if (!_cancelToken.IsCancellationRequested)
                return false;

            ErrorHandle.Logger.LogMessage(_msgCancel, LogLevel.Warning);
            result = new JobResult(JobStatus.Stoped, new AutoJobException(Current.Id, null, _msgCancel));
            return true;
        }
 
    }
}
