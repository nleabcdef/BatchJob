using BatchProcess.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using BatchProcess.AutoJob;

namespace BatchProcess.AutoJob.Runtime
{
    /// <summary>
    /// Workflow runner, executes jobs in sequential fashion
    /// </summary>
    public partial class SequentialRunner : IWorkflowHost<SequentialRuntime>, IWorkflowRunner
    {
        public IWorkflowJob Current { get; protected set; }
        public RuntimeType Type { get { return RuntimeType.Sequential; } }
        public Mutex mLock => Current.mLock;
        public SequentialRuntime Runtime => _runtime;

        protected List<IAutomatedJob> _workflow { get; set; }
        protected Task<JobResult> _task { get; private set; }
        protected SequentialRuntime _runtime { get; set; }

        private IWorkflowThread<JobResult> _workflowThread { get; set; }
        private CancellationToken _cancelToken;
        private JobId _currentJobId = null;
        private ConcurrentDictionary<string, JobStatus> _jobStatus { get; set; }
        private IJobContext _context { get; set; }
       
        public SequentialRunner(): this(null) { }
        public SequentialRunner(IWorkflowThread<JobResult> threadHandler = null)
        {
            _workflow = new List<IAutomatedJob>();
            _runtime = ServiceRepo.Instance.GetServiceOf<SequentialRuntime>();
            _workflowThread = threadHandler ?? _runtime.ServiceProvider.GetServiceOf<IWorkflowThread<JobResult>>();  // new WorkflowThread();
        }

        /// <summary>
        /// returns JobStatus for given job id, 
        /// returns the current status if the jobid found in this workflow.
        /// </summary>
        /// <param name="workflowId">JobId</param>
        /// <returns></returns>
        public JobStatus GetStatus(JobId workflowId)
        {
            if (workflowId.Equals(Current.Id))
                return _workflowThread.GetStatus(workflowId.ToString());

            if (_currentJobId != null && workflowId.Equals(_currentJobId))
                return JobStatus.Running;

            var key = workflowId.ToString();
            if (_jobStatus.ContainsKey(key))
                return _jobStatus[key];

            if (!_workflow.Exists(j => j.Id.Equals(workflowId)))
                throw new KeyNotFoundException("job id : " + workflowId.Id);

            return JobStatus.NotStarted;
        }

        /// <summary>
        /// get JobResult for given jobid.
        /// </summary>
        /// <param name="workflowId">JobId</param>
        /// <returns></returns>
        public JobResult GetResult(JobId workflowId)
        {
            if (workflowId.Equals(Current.Id))
                return _workflowThread.GetResult(workflowId.ToString());

            throw new KeyNotFoundException("job id : " + workflowId.Id);
        }

        /// <summary>
        /// Wait until all workflow jobs are complted their execution.
        /// </summary>
        /// <returns></returns>
        public bool WaitForAll()
        {
            return _workflowThread.WaitForAll();
        }

        /// <summary>
        /// starts the given IWorkflowJob and returns the runner to control/state management.
        /// </summary>
        /// <param name="workflow">IWorkflowJob</param>
        /// <returns></returns>
        public IWorkflowRunner Start(IWorkflowJob workflow)
        {
            Current = workflow ?? throw new ArgumentNullException("workflow");
            Init();

            _workflowThread.AddAndStart(
                () => StartWorkflow(),
                Current.Id.ToString(), ref _cancelToken);

            return this;
        }

        /// <summary>
        /// signle to stop all of its unfinished jobs
        /// </summary>
        public void SoftStop()
        {
            _workflowThread.StopAll();
        }

        public IWorkflowHost<IRuntime> AsHost()
        {
            return this;
        }

        private void Init()
        {
            _workflow.Clear();
            _workflow.AddRange(Current.Workflow);
            _jobStatus = new ConcurrentDictionary<string, JobStatus>();
            _currentJobId = null;
            _context = null;
        }
        
    }
}
