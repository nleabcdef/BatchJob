using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BatchProcess.AutoJob
{
    /// <summary>
    /// Builds workflow jobs using fluent interface.
    /// </summary>
    public sealed class JobBuilder
    {
        IWorkflowJob _workflow = null;
        public JobBuilder ThenAdd(IAutomatedJob job)
        {
            if (job == default(IAutomatedJob))
                throw new ArgumentNullException("job");

            _workflow.AddJob(job);

            return this;
        }

        public JobBuilder ThenAdd(IEnumerable<IAutomatedJob> jobs)
        {
            if (jobs == default(IAutomatedJob))
                throw new ArgumentNullException("job");

            if (jobs.Where(j => j == default(IAutomatedJob)).Count() > 0)
                throw new ArgumentNullException("job");

            _workflow.AddJobs(jobs.ToArray());

            return this;
        }

        public Creator NothingElse() { return new Creator(_workflow); }

        public JobBuilder(IWorkflowJob workflow)
        {
            _workflow = workflow ?? throw new ArgumentNullException("workflow");
        }

        public sealed class Creator
        {
            IWorkflowJob _workflow = null;
            public IAutomatedJob Create()
            {
                //todo : validate workflow

                return _workflow;
            }

            public Creator(IWorkflowJob workflow)
            {
                _workflow = workflow ?? throw new ArgumentNullException("workflow");
            }
        }
    }
}
