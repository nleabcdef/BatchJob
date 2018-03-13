using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BatchProcess.AutoJob.Runtime
{
    /// <summary>
    /// Internal - thread/task management helper for workflow jobs - thread safe
    /// </summary>
    internal class WorkflowThread : IWorkflowThread<JobResult>
    {
        private ConcurrentDictionary<string, Task<JobResult>> _tasks { get; set; }
        private CancellationTokenSource _cancellationSource { get; set; }
        private List<TaskStatus> _runningStatus { get; set; }

        public WorkflowThread()
        {
            _tasks = new ConcurrentDictionary<string, Task<JobResult>>();
            _runningStatus = new List<TaskStatus>() {
                TaskStatus.Running,
                TaskStatus.WaitingForActivation,
                TaskStatus.WaitingForChildrenToComplete,
                TaskStatus.WaitingToRun };
            _cancellationSource = new CancellationTokenSource();
        }

        public void AddAndStart(Func<JobResult> task, string id, ref CancellationToken token, bool addCancelToken = false)
        {
            if (task == null || string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException("task or id");

            if (_tasks.ContainsKey(id)) throw new ArgumentException("task id already exists");

            token = _cancellationSource.Token;
            var t = addCancelToken ? new Task<JobResult>(task, token) : new Task<JobResult>(task);

            _tasks[id] = t;
            t.Start();
        }

        public JobResult GetResult(string id)
        {
            if (!_tasks.ContainsKey(id)) throw new KeyNotFoundException("task id does not exists.");

            var task = _tasks[id];
            if (task.Status == TaskStatus.RanToCompletion)
                return task.Result;

            return null;
        }

        public JobStatus GetStatus()
        {
            if (_tasks.Values.Any(t => _runningStatus.Contains(t.Status)))
                return JobStatus.Running;

            if (_tasks.Values.Any(t =>
                 t.Status == TaskStatus.Canceled ||
                 t.Status == TaskStatus.Faulted))
                return JobStatus.Stoped;

            if (_tasks.Values.Any(t => t.Result.Status == JobStatus.CompletedWithError))
                return JobStatus.CompletedWithError;

            if (_tasks.Values.All(t => t.Result.Status == JobStatus.Completed))
                return JobStatus.Completed;

            return JobStatus.NotStarted;
        }

        public JobStatus GetStatus(string id)
        {
            if (!_tasks.ContainsKey(id)) throw new KeyNotFoundException("task id does not exists.");

            var task = _tasks[id];
            if (task.Status == TaskStatus.RanToCompletion)
                return task.Result.Status;

            return GetStatus(task.Status);
        }

        private JobStatus GetStatus(TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.Canceled:
                    return JobStatus.Stoped;

                case TaskStatus.Faulted:
                    return JobStatus.CompletedWithError;

                case TaskStatus.Running:
                case TaskStatus.WaitingForActivation:
                case TaskStatus.WaitingForChildrenToComplete:
                case TaskStatus.WaitingToRun:
                    return JobStatus.Running;
            }

            return JobStatus.NotStarted;
        }

        public void StopAll()
        {
            _cancellationSource.Cancel(true);
        }

        public bool WaitForAll()
        {
            var lst = _tasks.Values.Where(t => _runningStatus.Contains(t.Status));
            if (lst.Any())
            {
                Task[] arr = lst.ToArray();
                Task.WaitAll(arr);
            }
            return true;
        }
    }
}