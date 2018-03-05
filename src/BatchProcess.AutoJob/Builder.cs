using System;
using System.Collections.Generic;
using System.Text;

namespace BatchProcess.AutoJob
{
    /// <summary>
    /// Builds workflow jobs using fluent interface.
    /// </summary>
    public sealed class Builder
    {
        private IWorkflowJob _workflow = null;
        private JobId _id;
        private WhenFailure _failure = WhenFailure.StopOrExitJob;
        private ShareContext _share = ShareContext.Parent;
        private IJobContext _context;
        public JobBuilder AddJob(IAutomatedJob job)
        {
            if (job == default(IAutomatedJob))
                throw new ArgumentNullException("job");

            _workflow = new WorkflowJob(_id, _context, _failure, _share);
            _workflow.AddJob(job);

            return new JobBuilder(_workflow);
        }
        public Builder WithOption(WhenFailure onFailure, ShareContext share)
        {
            _failure = onFailure;
            _share = share;
            return this;
        }

        public Builder WithContext(IJobContext context)
        {
            _context = context ?? throw new ArgumentNullException("context");

            return this;
        }

        private Builder(JobId id, IJobContext context = null)
        {
            _id = id;
            _context = context ?? new JobContext(_id);
        }

        public static Builder BuildWorkflow(JobId id, IJobContext context = null)
        {
            if (id == default(JobId))
                throw new ArgumentNullException("id");

            return new Builder(id, context);
        }

    }
}
