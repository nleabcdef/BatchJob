using BatchProcess.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace BatchProcess.AutoJob.Runtime
{
    /// <summary>
    /// Workflow runner, executes jobs in parallel fashion
    /// </summary>
    public partial class TaskRunner : IWorkflowHost, IWorkflowRunner
    {
        public IWorkflowJob Current { get; protected set; }
        public RuntimeType Type { get { return RuntimeType.Parallel; } }
        public Mutex mLock => Current.mLock;
        
        protected List<IAutomatedJob> _workflow { get; set; }

        private IWorkflowThread<JobResult> _workflowThread { get; set; }
        private CancellationToken _cancelToken;
        private ConcurrentDictionary<string, JobResult> _jobStatus { get; set; }
        private JobResult _result { get; set; }
        
        private TaskRunner() { }
        public TaskRunner(IWorkflowThread<JobResult> threadHandler = null)
        {
            _workflow = new List<IAutomatedJob>();
            _workflowThread = threadHandler ?? new WorkflowThread();
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
        /// returns JobStatus for given job id, 
        /// returns the current status if the jobid found in this workflow.
        /// </summary>
        /// <param name="workflowId">JobId</param>
        /// <returns></returns>
        public JobStatus GetStatus(JobId workflowId)
        {
            if (workflowId.Equals(Current.Id))
                return _workflowThread.GetStatus();

            if (_jobStatus.ContainsKey(workflowId.ToString()))
                return _jobStatus[workflowId.ToString()].Status;

            if (!_workflow.Exists(j => j.Id.Equals(workflowId)))
                throw new KeyNotFoundException("JobId");

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
            {
                if (_workflowThread.WaitForAll())
                    _result = new JobResult(_workflowThread.GetStatus());

                return _result;
            }
            if (_jobStatus.ContainsKey(workflowId.ToString()))
                return _jobStatus[workflowId.ToString()];

            return null;
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
            StartWorkflow();

            return this;
        }

        /// <summary>
        /// signle to stop all of its unfinished jobs
        /// </summary>
        public void SoftStop()
        {
            _workflowThread.StopAll();
        }

        private void Init()
        {
            _result = null;
            _workflow.Clear();
            _workflow.AddRange(Current.Workflow);
            _jobStatus = new ConcurrentDictionary<string, JobResult>();
        }
        
    }
}
