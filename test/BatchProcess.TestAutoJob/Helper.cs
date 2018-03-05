using BatchProcess.AutoJob;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Moq;
using System.Linq;

namespace BatchProcess.TestAutoJob
{
    internal class Helper
    {
        public static JobId GetJobId(string name = "Default-Job")
        {
            return new JobId(Guid.NewGuid().ToString().Substring(0, 10), name);
        }
        
        public static WorkflowJob GetWorkflowJob(JobId id=null, WhenFailure ifFailure = WhenFailure.ContinueOn, ShareContext share = ShareContext.Parent, int noOfJobs = 0, JobStatus status = JobStatus.Completed, bool longProcess = false)
        {
            if (id == null)
                id = GetJobId();

            var context = new Mock<IJobContext>();
            context.Setup(c => c.ParentJobId).Returns(id);

            var rtn = new WorkflowJob(id, context.Object, ifFailure, share);
            
            for(int i =0; i < noOfJobs; i++)
            {
                rtn.AddJob(
                    longProcess? 
                    GetLongRunningJob(
                        TimeSpan.FromMilliseconds(500), 
                        null, status)
                    .Object :
                    GetFakeJob(null, status)
                    .Object);
            }

            return rtn;
        }

        public static WorkflowJob GetNestedWorkflow(int noOfJobs=2)
        {
            JobId id = GetJobId();

            var context = new Mock<IJobContext>();
            context.Setup(c => c.ParentJobId).Returns(id);

            var rtn = new WorkflowJob(id, context.Object, WhenFailure.ContinueOn, ShareContext.Parent);
            var rn = new Random();
            for (int i = 0; i < noOfJobs; i++)
            {
                var n = rn.Next(10);
                n = n == 0 ? 2 : n;
                rtn.AddJob(GetWorkflowJob(noOfJobs : n, longProcess : true));
            }

            return rtn;
        }

        public static WorkflowJob GetNestedWorkflowWithRandomStatus(TimeSpan timeToRun, int noOfJobs = 2)
        {
            JobId id = GetJobId();

            var context = new Mock<IJobContext>();
            context.Setup(c => c.ParentJobId).Returns(id);

            var rtn = new WorkflowJob(id, context.Object, WhenFailure.ContinueOn, ShareContext.Parent);
            var rn = new Random();
            for (int i = 0; i < noOfJobs; i++)
            {
                var n = rn.Next(10);
                n = n == 0 ? 2 : n;
                rtn.AddJob(GetWorkflowJob(noOfJobs: n, longProcess: true, status: n%2 ==0 ? JobStatus.Completed: JobStatus.CompletedWithError));
            }

            return rtn;
        }

        public static Mock<IFakeJob> GetFakeJob(JobId id = null, JobStatus status = JobStatus.Completed)
        {
            if (id == null)
                id = GetJobId();

            var rtn = new Mock<IFakeJob>();
            rtn.Setup(j => j.Id).Returns(id);

            var context = new Mock<IJobContext>();
            context.Setup(c => c.ParentJobId).Returns(id);
            rtn.Setup(j => j.Context).Returns(context.Object);
            rtn.Setup(j => j.mLock).Returns(new Mutex());

            var rslt = new JobResult(status, status == JobStatus.CompletedWithError ? new AutoJobException(id) : null);
            rtn.Setup(j => j.Doable()).Returns(rslt);

            return rtn;
        }
        
        public static Mock<IFakeJob> GetLongRunningJob(TimeSpan timeToRun, JobId id = null, JobStatus status = JobStatus.Completed)
        {
            if (id == null)
                id = GetJobId();

            var rtn = new Mock<IFakeJob>();
            rtn.Setup(j => j.Id).Returns(id);

            var context = new Mock<IJobContext>();
            context.Setup(c => c.ParentJobId).Returns(id);
            rtn.Setup(j => j.Context).Returns(context.Object);
            rtn.Setup(j => j.mLock).Returns(new Mutex());

            var rslt = new JobResult(status, status == JobStatus.CompletedWithError ? new AutoJobException(id) : null);
            rtn.Setup(j => j.Doable())
                .Callback(() => Thread.Sleep(timeToRun.Milliseconds))
                .Returns(rslt);

            return rtn;
        }


    }

    public interface IFakeJob : IAutomatedJob
    { }
    
}
