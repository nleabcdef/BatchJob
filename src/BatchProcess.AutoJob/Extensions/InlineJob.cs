using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BatchProcess.AutoJob.Extensions
{
    /// <summary>
    /// Wrapper to create jobs inline syntax
    /// </summary>
    public class InlineJob : IAutomatedJob
    {
        public JobId Id { get; protected set; }

        public IJobContext Context { get; set; }

        public Mutex mLock => _lock;
        private Mutex _lock = new Mutex();
        private Func<IJobContext, JobResult> _inline { get; set; }
        public virtual JobResult Doable()
        {
            return _inline(Context);
        }

        private InlineJob() { }
        public InlineJob(JobId id, Func<IJobContext, JobResult> inline, IJobContext context = null)
        {

            Id = id ?? throw new ArgumentNullException("id");
            Context = context ?? new JobContext(Id);

            JobResult inl(IJobContext ctx)
            { return new JobResult(JobStatus.Completed); }
            _inline = inline ?? inl;

        }

        public InlineJob(JobId id, Action<IJobContext> inline, IJobContext context = null)
        {

            Id = id ?? throw new ArgumentNullException("id");
            Context = context ?? new JobContext(Id);

            JobResult inl(IJobContext ctx)
            { inline?.Invoke(Context); return new JobResult(JobStatus.Completed); }

            _inline = inl;

        }

        public static InlineJob GetDefault(Func<IJobContext, JobResult> action)
        {
            return new InlineJob(GetJobId(), action);
        }

        public static InlineJob GetDefault(Action<IJobContext> action)
        {
            return new InlineJob(GetJobId(), action);
        }

        public static JobId GetJobId(string id = null, string name = null)
        {
            return new JobId(id ?? Guid.NewGuid().ToString().Substring(12), name ?? "Job-Name-Default");
        }
    }
}
