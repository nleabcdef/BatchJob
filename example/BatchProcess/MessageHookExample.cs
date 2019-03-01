using BatchProcess.AutoJob;
using BatchProcess.AutoJob.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BatchProcess
{
    /// <summary>
    /// Example to demonstrate the Message hook functionality
    /// Updated in Readme file
    /// </summary>
    internal class MessageHookExample
    {
        public void RunAll()
        {
            With_Workflow_Job();
            With_Standalone_Job();
        }

        public void With_Standalone_Job()
        {
            //setup Download job
            IAutomatedJob _download = InlineJob.GetDefault((ctx) =>
            {
                //push
                ctx.PushReportToHookAsync(ctx.ParentJobId,
                    new MessageHook("Download Started", "Info", MessageType.Info));

                // download the data
                Task.Delay(200).Wait();

                //push
                ctx.PushReportToHookAsync(ctx.ParentJobId,
                    new MessageHook("Download Completed", "Info", MessageType.Info));

                return new JobResult(JobStatus.Completed);
            });

            //get notofication manager, from service repo
            var manager = ServiceRepo.Instance.GetServiceOf<INotificationManager<JobId>>();

            //setup Message hooks
            var hanlder = new AggregateHandler();
            manager.RegisterHook(_download.Id, MessageType.Info, hanlder);

            var result = _download.Doable(); //execute the job

            Console.WriteLine(result.Status.ToString()); //Completed

            //print the messages recieved
            hanlder.MessagesRecieved.ToList().ForEach(m =>
            {
                Console.WriteLine(m.Item2.ToString());
            });
        }

        public void With_Workflow_Job()
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

            //get notofication manager, from service repo
            var manager = ServiceRepo.Instance.GetServiceOf<INotificationManager<JobId>>();

            //setup Message hooks
            var hanlder = new AggregateHandler();
            manager.RegisterHook(_download.Id, MessageType.Info, hanlder);
            manager.RegisterHook(_getAuthToken.Id, MessageType.Info, hanlder);

            var result = _integraion.Doable(); //run the integration job

            Console.WriteLine(result.Status.ToString()); //Completed

            //print the messages recieved
            hanlder.MessagesRecieved.ToList().ForEach(m =>
            {
                Console.WriteLine(m.Item2.ToString());
            });

        }
    }

    /// <summary>
    /// Sample notification handler, to aggregate messages when it arraives
    /// </summary>
    public class AggregateHandler : IHookHandler<JobId>
    {
        public string Id => "id-of-AggregateHandler";
        public string Name => "Aggregate-Handler";
        IList<Task<bool>> _handles { get; set; }
        public IEnumerable<Tuple<JobId, MessageHook>> MessagesRecieved
        {
            get
            {
                Task.WaitAll(_handles.ToArray());
                new TaskFactory().StartNew(() => {
                    lock(_handles)
                    {
                        _handles = _handles.Where(t => !t.IsCompleted).ToList();
                    }
                });
                return _messages.AsEnumerable();
            }
        }
        protected ConcurrentQueue<Tuple<JobId, MessageHook>> _messages { get; set; }

        public Task<bool> DoHandle(JobId sender, MessageHook message)
        {
            var tsk = new Task<bool>(() =>
            {
                _messages.Enqueue(new Tuple<JobId, MessageHook>(sender, message));
                return true;
            });
            lock (_handles) { _handles.Add(tsk); }
            
            tsk.Start();
            return tsk;
        }
        
        public AggregateHandler()
        {
            _messages = new ConcurrentQueue<Tuple<JobId, MessageHook>>();
            _handles = new List<Task<bool>>();
        }
    }
}