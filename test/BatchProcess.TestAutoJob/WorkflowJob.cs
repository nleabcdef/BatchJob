using System;
using Xunit;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BatchProcess.AutoJob;
using Moq;

namespace BatchProcess.TestAutoJob
{
    public class TestWorkflowJob
    {
        [Fact]
        public void Workflow_with_5_sequential_autojobs()
        {
            //arrange
            var workflow = Helper.GetWorkflowJob(null, WhenFailure.StopOrExitJob, ShareContext.Parent, 5);

            //act
            var rslt = workflow.Doable();

            //assert
            Assert.NotNull(workflow.Id);
            Assert.True(workflow.Workflow.Count == 5);
            Assert.All(workflow.Workflow, j => workflow.GetJobStatus(j.Id).Equals(JobStatus.Completed.ToString()));
            
        }

        [Fact]
        public void Workflow_with_5_sequential_autojobs_has_failure()
        {
            //arrange
            var workflow = Helper.GetWorkflowJob(null, WhenFailure.ContinueOn, ShareContext.Parent, 5, JobStatus.CompletedWithError);

            //act
            var rslt = workflow.Doable();

            //assert
            Assert.NotNull(workflow.Id);
            Assert.True(rslt.Status == JobStatus.CompletedWithError);
            Assert.True(workflow.Workflow.Count == 5);
            Assert.All(workflow.Workflow, j => workflow.GetJobStatus(j.Id).Equals(JobStatus.CompletedWithError.ToString()));

        }

        [Fact]
        public void Workflow_with_5_sequential_autojobs_has_failure_aborted()
        {
            //arrange
            var workflow = Helper.GetWorkflowJob(null, WhenFailure.StopOrExitJob, ShareContext.Parent, 5, JobStatus.CompletedWithError);

            //act
            var rslt = workflow.Doable();

            //assert
            Assert.NotNull(workflow.Id);
            Assert.True(rslt.Status == JobStatus.CompletedWithError);
            Assert.True(workflow.Workflow.Count == 5);
            Assert.All(workflow.Workflow.Take(1), j => workflow.GetJobStatus(j.Id).Equals(JobStatus.CompletedWithError.ToString()));
            Assert.All(workflow.Workflow.TakeLast(4), j => workflow.GetJobStatus(j.Id).Equals(JobStatus.NotStarted.ToString()));

        }

        [Fact]
        public void Workflow_with_5_sequential_workflowjob_nested()
        {
            //arrange
            var workflow = Helper.GetWorkflowJob(null, WhenFailure.StopOrExitJob, ShareContext.Parent, 0);
            var rn = new Random();
            for (int i=0; i <5; i++)
            {
                var wf = Helper.GetWorkflowJob(null, WhenFailure.StopOrExitJob, ShareContext.Parent, rn.Next(5));
                workflow.AddJob(wf);
            }

            //act
            var rslt = workflow.Doable();

            //assert
            Assert.NotNull(workflow.Id);
            Assert.True(workflow.Workflow.Count == 5);
            Assert.All(workflow.Workflow, j => workflow.GetJobStatus(j.Id).Equals(JobStatus.Completed.ToString()));

        }

        [Fact]
        public void Workflow_with_5_sequential_workflowjob_nested_aborted()
        {
            //arrange
            var workflow = Helper.GetWorkflowJob(null, WhenFailure.StopOrExitJob, ShareContext.Parent, 0);
            
            for (int i = 0; i < 3; i++)
            {
                var wf = Helper.GetWorkflowJob(null, WhenFailure.ContinueOn, ShareContext.Parent, i+1);
                workflow.AddJob(wf);
            }
            var wfailed = Helper.GetWorkflowJob(new JobId("1-1-1", "failedJob") , WhenFailure.ContinueOn, ShareContext.Parent, 1, JobStatus.CompletedWithError);
            workflow.AddJob(wfailed);
            var lstNotStarted = new List<IAutomatedJob>();
            for (int i = 0; i < 2; i++)
            {
                var wf1 = Helper.GetWorkflowJob(null, WhenFailure.ContinueOn, ShareContext.Parent, i+2);
                lstNotStarted.Add(wf1);
                workflow.AddJob(wf1);
            }

            //act
            var rslt = workflow.Doable();
            
            //assert
            Assert.NotNull(workflow.Id);
            Assert.True(rslt.Status == JobStatus.CompletedWithError);
            Assert.True(workflow.Workflow.Count == 6);
            Assert.All(lstNotStarted, j => workflow.GetJobStatus(j.Id).Equals(JobStatus.NotStarted.ToString()));
            Assert.Equal(JobStatus.CompletedWithError, workflow.GetJobStatus(wfailed.Id));

        }

    }
}
