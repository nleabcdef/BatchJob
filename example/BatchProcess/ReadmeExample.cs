using BatchProcess.AutoJob;
using BatchProcess.AutoJob.Extensions;
using BatchProcess.Shared;
using System;
using System.Threading.Tasks;

namespace BatchProcess
{
    /// <summary>
    /// examples of workflow job processing,
    /// updated in Readme file
    /// </summary>
    internal class ReadmeExample
    {
        public void RunAll()
        {
            WorkflowJob();
            RetryJob();
        }

        public void RetryJob()
        {
            IAutomatedJob _processFile = InlineJob.GetDefault((ctx) =>
            {
                //to simulate the retry when job failed
                throw new Exception("making the job failed");

                return new JobResult(JobStatus.Completed);
            });

            //make the job as retry, when error/exception is anticipated
            var retry = _processFile.ConvertToRetry(3, TimeSpan.FromMilliseconds(500));

            ErrorHandle.Logger = NullLogger.Instance; //disable the default console logging

            var rResult = retry.Doable(); // run retry job

            Console.WriteLine(rResult.Status); //CompletedWithError
            foreach (var r in retry.RetryResults)
                Console.WriteLine(r.Error.Message); //print retry error/excpetion message
        }

        public void WorkflowJob()
        {
            IAutomatedJob _getAuthToken = InlineJob.GetDefault((ctx) =>
            {
                //call actual api and get Auth token
                var token = Guid.NewGuid().ToString();

                //store the token in to Context store
                ctx.SetValue(token, "token-key");
                //Console.WriteLine(token);

                return new JobResult(JobStatus.Completed);
            });

            IAutomatedJob _download = InlineJob.GetDefault((ctx) =>
            {
                //get Auth token from Context store
                var token = ctx.GetValue<string>("token-key");
                //Console.WriteLine(token);

                // download the data using Auth token
                Task.Delay(200).Wait();

                return new JobResult(JobStatus.Completed);
            });

            //build integraion workflow - sequential by default - parallel also be possible
            IAutomatedJob _integraion = Builder.BuildWorkflow(InlineJob.GetJobId())
                .WithOption(WhenFailure.StopOrExitJob, ShareContext.previous)
                .AddJob(_getAuthToken)
                .ThenAdd(_download)
                .NothingElse().Create();

            var result = _integraion.Doable(); //run the integration job

            Console.WriteLine(result.Status.ToString()); //Completed
        }
    }
}