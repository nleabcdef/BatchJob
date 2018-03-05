using BatchProcess.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;

namespace BatchProcess.AutoJob
{
    /// <summary>
    /// Decorates the IAutomatedJob with retry functionality
    /// </summary>
    /// <typeparam name="TJob"></typeparam>
    public class Retry<TJob> : IAutomatedJob
        where TJob : IAutomatedJob
    {
        public int Times { get; }
        public TimeSpan Interval { get; }
        public IReadOnlyList<JobResult> RetryResults => _retryResults.AsReadOnly();
        public Mutex mLock => _lock;

        protected List<JobResult> _retryResults { get; set; }
        protected TJob _job = default(TJob);
        protected Func<IJobContext, ValidationResult> BeforeRetry { get; set; }
        protected int CurrentRetry { get; set; }

        private Mutex _lock = new Mutex();
        private const int _minRetry = 2;
        private const string _msgRetry = "times should always be grater then or equal to {0}.";
        private const string _msgValidationFailure = "Validation Failed on retry attempt: {0}";
        private const string _msgError = "Error occurred in Retry Job.";
        private const string _msgJobError = "Error < Retry Job of id: {0}, name: {1}>.";
        private const string _msgRetryError = "Warning < current retry count: {0}, total: {1}>.";


        private Retry() { }
        
        public Retry(TJob job, int times, TimeSpan interval, Func<IJobContext, ValidationResult> doBeforeRetry = null)
        {
            if(job == null)
                throw new ArgumentNullException("job");

            _job = job;
            Times = times < _minRetry ? throw new ArgumentOutOfRangeException(string.Format(_msgRetry, _minRetry)) : times;
            Interval = interval;
            BeforeRetry = doBeforeRetry;
            _retryResults = new List<JobResult>();
        }

        /// <summary>
        /// unique identifier
        /// </summary>
        public JobId Id { get { return _job.Id; } }

        /// <summary>
        /// jobcontext
        /// </summary>
        public IJobContext Context
        {
            get
            {
                return _job.Context;
            }
            set
            {
                if (_job.mLock.WaitOne())
                {
                    _job.Context = value;
                    _job.mLock.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// wrap the retry functionality into the given job.
        /// </summary>
        /// <returns></returns>
        public virtual JobResult Doable()
        {
            JobResult rtn = null;

            Mutex[] m = new Mutex[2] { _job.mLock, mLock };
            if (Mutex.WaitAll(m))
            {

                rtn = ErrorHandle.Expect(Invoke, out bool anyError,
                    string.Format(_msgJobError, Id.Id, Id.Name),
                    _msgError);
                foreach (var mtx in m)
                {
                    mtx.ReleaseMutex();
                }
                if(anyError)
                    rtn = new JobResult(_retryResults.Last().Status);
            }

            rtn = rtn?? new JobResult(_retryResults.Last().Status);

            return rtn;
        }

        private JobResult Invoke()
        {
            JobResult rtn = null;

            while (!DoRetry(out rtn))
            {
                _retryResults.Add(rtn);
                rtn = null;
                Thread.Sleep((int)Interval.TotalMilliseconds);

                if (Validate())
                    continue;

                rtn = new JobResult(
                            JobStatus.CompletedWithError,
                            new AutoJobException(_job.Id,
                            null,
                            string.Format(_msgValidationFailure, CurrentRetry)));
                break;

            }
            if (rtn != null)
                _retryResults.Add(rtn);

            return rtn;
        }

        private bool Validate()
        {
            if (BeforeRetry == null)
                return true;

            var rs = ErrorHandle.Expect(
            () =>
            {
                return BeforeRetry(_job.Context);

            }, out bool anyError, string.Format(_msgValidationFailure, CurrentRetry));

            return !(rs == ValidationResult.NotValid || anyError);
        }

        protected bool DoRetry(out JobResult result)
        {
            result = null;

            if (CurrentRetry < Times)
            {
                CurrentRetry++;
                result = ErrorHandle.Expect(() => { return _job.Doable(); },
                    out bool anyError,
                    _msgError,
                    string.Format(_msgJobError, Id.Id, Id.Name),
                    string.Format(_msgRetryError, CurrentRetry, Times));

                if (anyError)
                {
                    result = new JobResult(JobStatus.CompletedWithError,
                        new AutoJobException(_job.Id, null, 
                        string.Format(_msgRetryError, CurrentRetry, Times)));
                }
            }
            else
                return true;

            if (Interval == TimeSpan.Zero)
                return false;

            return result.Status == JobStatus.Completed;
        }
    }
}
