using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using BatchProcess.Shared;
using System.Diagnostics;
using BatchProcess.AutoJob.Runtime;

namespace BatchProcess.AutoJob
{
    /// <summary>
    /// Adds, workflow functionalities to IAutomatedJob
    /// </summary>
    public class WorkflowJob : IWorkflowJob
    {
        public IReadOnlyList<IAutomatedJob> Workflow => _workflow.AsReadOnly();
        public WhenFailure OnFailure { get; protected set; }
        public ShareContext Option { get; protected set; }
        public JobId Id { get; protected set; }
        public Mutex mLock => _lock;
        
        protected IWorkflowHost<IRuntime> _host { get; set; }
        protected IWorkflowRunner _runner { get; set; }

        protected List<IAutomatedJob> _workflow { get; set; }
        
        private const RuntimeType _runtimeType = RuntimeType.Sequential; 
        private Mutex _lock = new Mutex();
        private IJobContext _context { get; set; }
        private const string _msgContextError = "coudl not set, Context. mLock handle is not acquired.";

        private WorkflowJob() { }

        public WorkflowJob(JobId id, IJobContext context, WhenFailure onFailure,
            ShareContext share, IWorkflowHost<IRuntime> host = null)
        {
            if (id == default(JobId) || context == default(IJobContext))
                throw new ArgumentNullException("id, context");

            Id = id;
            Context = context;
            OnFailure = onFailure;
            Option = share;
            _host = host ?? ServiceRepo.Instance.GetServiceOf<IWorkflowHost<SequentialRuntime>>()?.AsHost();
            _runner = null;
            _workflow = new List<IAutomatedJob>();
        }

        /// <summary>
        /// job context, used to share data/objects between jobs
        /// </summary>
        public IJobContext Context
        {
            get => _context;
            set
            {
                if (mLock.WaitOne())
                {
                    _context = value;
                    mLock.ReleaseMutex();
                }
                else
                    ErrorHandle.Logger.LogMessage(_msgContextError, LogLevel.Warning);
            }
        }

        /// <summary>
        /// Add the given job into workflow
        /// </summary>
        /// <param name="job">IAutomatedJob</param>
        /// <returns>returns the job count, 0 otherwise</returns>
        public int AddJob(IAutomatedJob job)
        {
            if (job == default(IAutomatedJob))
                throw new ArgumentNullException("job");
            if (mLock.WaitOne())
            {
                _workflow.Add(job);
                mLock.ReleaseMutex();
                return _workflow.Count;
            }

            return 0;
        }

        /// <summary>
        /// add given jobs array into workflow
        /// </summary>
        /// <param name="jobs">array of jobs</param>
        /// <returns>returns the job count, 0 otherwise</returns>
        public int AddJobs(params IAutomatedJob[] jobs)
        {
            if (jobs == null || jobs.Where(j => j == default(IAutomatedJob)).Count() > 0)
                throw new ArgumentNullException("jobs");
            if (mLock.WaitOne())
            {
                foreach (var job in jobs)
                {
                    _workflow.Add(job);
                }
                mLock.ReleaseMutex();
                return _workflow.Count;
            }

            return 0;
        }

        /// <summary>
        /// executes the workflow jobs, using WorkflowHost with specified RuntimeType.
        /// </summary>
        /// <returns>JobResult</returns>
        public virtual JobResult Doable()
        {
            _runner = _host.Start(this);
            if (_runner.WaitForAll())
                return _runner.GetResult(this.Id);

            return null;
        }

        /// <summary>
        /// removes the job, if there is a match in workflow collection, exception otherwise
        /// </summary>
        /// <param name="id">JobId</param>
        public void RemoveJob(JobId id)
        {
            var job = _workflow.Where(c => c.Id.Equals(id)).FirstOrDefault();
            if (job != null)
            {
                if (mLock.WaitOne())
                {
                    _workflow.Remove(job);
                    mLock.ReleaseMutex();
                }
            }
            else
                throw new KeyNotFoundException(string.Format("id {0}", id.Id));

        }

        /// <summary>
        /// returns the workflow status, of recent execution
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JobStatus GetJobStatus(JobId id)
        {
            var has = _workflow.Exists(j => j.Id.Equals(id));
            if (!has)
                throw new KeyNotFoundException("job id : " + id.Id);

            if (_runner == null)
                return JobStatus.NotStarted;

            return _runner.GetStatus(id);
        }
    }

}
