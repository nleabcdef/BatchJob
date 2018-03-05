using System;
using System.Collections.Generic;
using System.Text;

namespace BatchProcess.AutoJob
{
    public interface IWorkflowJob : IAutomatedJob
    {
        IReadOnlyList<IAutomatedJob> Workflow { get; }
        WhenFailure OnFailure { get; }
        ShareContext Option { get; }
        int AddJob(IAutomatedJob job);
        void RemoveJob(JobId id);
        JobStatus GetJobStatus(JobId id);
        int AddJobs(params IAutomatedJob[] jobs);
    }
}
