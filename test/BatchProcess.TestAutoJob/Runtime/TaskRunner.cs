using BatchProcess.AutoJob;
using System;
using Xunit;
using System.Linq;
using BatchProcess.TestAutoJob;
using BatchProcess.AutoJob.Runtime;
using System.Threading.Tasks;
using System.Threading;

namespace BatchProcess.TestAutoJob.Runtime
{
    public class TestTaskRunner
    {
        [Fact]
        public void Start_with_nested_longrunning_jobs()
        {
            //arrange
            var workflow = Helper.GetNestedWorkflow(5);

            //act
            IWorkflowRunner runner = new TaskRunner().Start(workflow);

            //assert
            Assert.NotNull(runner);
            Assert.NotNull(runner.Current);
            Assert.Equal(workflow, runner.Current);
            Assert.True(runner.GetStatus(workflow.Id) == JobStatus.Running);

        }

        [Fact]
        public void Start_with_nested_longrunning_jobs_has_random_failures()
        {
            //arrange
            var workflow = Helper.GetNestedWorkflowWithRandomStatus(TimeSpan.FromMilliseconds(1000), 10);

            //act
            IWorkflowRunner runner = new TaskRunner().Start(workflow);

            //assert
            Assert.NotNull(runner);
            Assert.NotNull(runner.Current);
            Assert.Equal(workflow, runner.Current);
            Assert.True(runner.GetStatus(workflow.Id) == JobStatus.Running);

        }
        
        [Fact]
        public void WaitForAll_with_longrunning_jobs()
        {
            //arrange
            var job = Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(100)).Object;
            var job1 = Helper.GetLongRunningJob(TimeSpan.FromSeconds(2)).Object;
            IWorkflowJob workflow = Builder.BuildWorkflow(Helper.GetJobId())
                .WithOption(WhenFailure.ContinueOn, ShareContext.Parent)
                .AddJob(job)
                .ThenAdd(job1)
                .ThenAdd(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetFakeJob().Object)
                .NothingElse().Create() as IWorkflowJob;

            //act
            IWorkflowRunner runner = new TaskRunner().Start(workflow);
            runner.WaitForAll();

            //assert
            Assert.NotNull(runner);
            Assert.NotNull(runner.Current);
            Assert.Equal(workflow, runner.Current);
            Assert.True(runner.GetStatus(workflow.Id) == JobStatus.Completed);
        }

        [Fact]
        public void WaitForAll_with_longrunning_jobs_has_failure()
        {
            //arrange
            var job = Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(100)).Object;
            var job1 = Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(400)).Object;
            IWorkflowJob workflow = Builder.BuildWorkflow(Helper.GetJobId())
                .WithOption(WhenFailure.ContinueOn, ShareContext.Parent)
                .AddJob(job)
                .ThenAdd(job1)
                .ThenAdd(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetFakeJob(status: JobStatus.CompletedWithError).Object)
                .ThenAdd(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetFakeJob().Object)
                .NothingElse().Create() as IWorkflowJob;

            //act
            IWorkflowRunner runner = new TaskRunner().Start(workflow);
            runner.WaitForAll();

            //assert
            Assert.NotNull(runner);
            Assert.NotNull(runner.Current);
            Assert.Equal(workflow, runner.Current);
            Assert.True(runner.GetStatus(workflow.Id) == JobStatus.CompletedWithError);
        }

        [Fact]
        public void WaitForAll_with_longrunning_jobs_have_failed()
        {
            //arrange
            var job = Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(100)).Object;
            var job1 = Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(400)).Object;
            IWorkflowJob workflow = Builder.BuildWorkflow(Helper.GetJobId())
                .WithOption(WhenFailure.StopOrExitJob, ShareContext.Parent)
                .AddJob(job)
                .ThenAdd(job1)
                .ThenAdd(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetFakeJob(status: JobStatus.CompletedWithError).Object)
                .ThenAdd(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetFakeJob().Object)
                .NothingElse().Create() as IWorkflowJob;

            //act
            IWorkflowRunner runner = new TaskRunner().Start(workflow);
            runner.WaitForAll();

            //assert
            Assert.NotNull(runner);
            Assert.NotNull(runner.Current);
            Assert.Equal(workflow, runner.Current);
            Assert.True(runner.GetStatus(workflow.Id) == JobStatus.CompletedWithError);
        }

        [Fact]
        public void GetStatus_returns_completion()
        {
            //arrange
            IWorkflowJob workflow = Builder.BuildWorkflow(Helper.GetJobId())
                .WithOption(WhenFailure.StopOrExitJob, ShareContext.Parent)
                .AddJob(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetWorkflowJob(noOfJobs: 2))
                .ThenAdd(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetWorkflowJob(noOfJobs: 5))
                .ThenAdd(Helper.GetFakeJob().Object)
                .NothingElse().Create() as IWorkflowJob;

            //act
            IWorkflowRunner runner = new TaskRunner().Start(workflow);
            runner.WaitForAll();
            var actual = runner.GetStatus(workflow.Id);

            //assert
            Assert.NotNull(runner);
            Assert.NotNull(runner.Current);
            Assert.Equal(workflow, runner.Current);
            Assert.True(actual == JobStatus.Completed);
        }

        [Fact]
        public void GetStatus_returns_failure()
        {
            //arrange
            IWorkflowJob workflow = Builder.BuildWorkflow(Helper.GetJobId())
                .WithOption(WhenFailure.StopOrExitJob, ShareContext.Parent)
                .AddJob(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetWorkflowJob(noOfJobs: 2))
                .ThenAdd(Helper.GetFakeJob(status: JobStatus.CompletedWithError).Object)
                .ThenAdd(Helper.GetWorkflowJob(noOfJobs: 5))
                .ThenAdd(Helper.GetFakeJob().Object)
                .NothingElse().Create() as IWorkflowJob;

            //act
            IWorkflowRunner runner = new TaskRunner().Start(workflow);
            runner.WaitForAll();
            var actual = runner.GetStatus(workflow.Id);

            //assert
            Assert.NotNull(runner);
            Assert.NotNull(runner.Current);
            Assert.Equal(workflow, runner.Current);
            Assert.True(actual == JobStatus.CompletedWithError);
        }

        [Fact]
        public void GetResult_with_longrunning_jobs_completed()
        {
            //arrange
            IWorkflowJob workflow = Builder.BuildWorkflow(Helper.GetJobId())
                .WithOption(WhenFailure.StopOrExitJob, ShareContext.Parent)
                .AddJob(Helper.GetLongRunningJob(TimeSpan.FromSeconds(1)).Object)
                .ThenAdd(Helper.GetWorkflowJob(noOfJobs: 5))
                .ThenAdd(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetLongRunningJob(TimeSpan.FromSeconds(2)).Object)
                .ThenAdd(Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(1)).Object)
                .ThenAdd(Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(1)).Object)
                .ThenAdd(Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(1)).Object)
                .ThenAdd(Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(1)).Object)
                .ThenAdd(Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(10)).Object)
                .ThenAdd(Helper.GetFakeJob().Object)
                .NothingElse().Create() as IWorkflowJob;

            //act
            IWorkflowRunner runner = new TaskRunner().Start(workflow);
            runner.WaitForAll();
            var actual = runner.GetResult(workflow.Id);

            //assert
            Assert.NotNull(runner);
            Assert.NotNull(runner.Current);
            Assert.Equal(workflow, runner.Current);
            Assert.True(actual.Status == JobStatus.Completed);
            Assert.Null(actual.Error);
        }

        [Fact]
        public void GetResult_with_longrunning_jobs_failure()
        {
            //arrange
            IWorkflowJob workflow = Builder.BuildWorkflow(Helper.GetJobId())
                .WithOption(WhenFailure.StopOrExitJob, ShareContext.Parent)
                .AddJob(Helper.GetLongRunningJob(TimeSpan.FromSeconds(1)).Object)
                .ThenAdd(Helper.GetWorkflowJob(noOfJobs: 5))
                .ThenAdd(Helper.GetFakeJob().Object)
                .ThenAdd(Helper.GetLongRunningJob(TimeSpan.FromSeconds(2)).Object)
                .ThenAdd(Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(1)).Object)
                .ThenAdd(Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(1)).Object)
                .ThenAdd(Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(1)).Object)
                .ThenAdd(Helper.GetLongRunningJob(TimeSpan.FromMilliseconds(1)).Object)
                .ThenAdd(Helper.GetFakeJob(status: JobStatus.CompletedWithError).Object)
                .ThenAdd(Helper.GetFakeJob().Object)
                .NothingElse().Create() as IWorkflowJob;

            //act
            IWorkflowRunner runner = new TaskRunner().Start(workflow);
            runner.WaitForAll();
            var actual = runner.GetResult(workflow.Id);

            //assert
            Assert.NotNull(runner);
            Assert.NotNull(runner.Current);
            Assert.Equal(workflow, runner.Current);
            Assert.True(actual.Status == JobStatus.CompletedWithError);
            Assert.Null(actual.Error);
        }
    }
}
