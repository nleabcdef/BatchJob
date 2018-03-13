using System;

namespace BatchProcess.AutoJob.Runtime
{
    /// <summary>
    /// Hosts workflow job, with either sequential or prallel execution of jobs
    /// This will be deprecated, in next up coming minor release.
    /// </summary>
    [Obsolete("WokflowHost will be deprecated, consider to use SequentialRunner or TaskRunner from BatchProcess.AutoJob.Runtime.", false)]
    public sealed class WorkflowHost : IWorkflowHost<IRuntime>
    {
        private WorkflowHost() { }

        /// <summary>
        /// This will be deprecated, in next up coming minor release.
        /// </summary>
        /// <param name="runtimeType"></param>
        public WorkflowHost(RuntimeType runtimeType)
        {
            Type = runtimeType;
        }

        public RuntimeType Type { get; }

        /// <summary>
        /// always returns default or null, because WorkflowHost is Obsolete
        /// </summary>
        public IRuntime Runtime => default(IRuntime);

        public IWorkflowRunner Start(IWorkflowJob workflow)
        {
            IWorkflowRunner runner = null;
            switch(Type)
            {
                case RuntimeType.Sequential:
                    runner = new SequentialRunner().Start(workflow);
                    break;
                case RuntimeType.Parallel:
                    runner = new TaskRunner().Start(workflow);
                    break;
            }
            
            return runner;
        }

        public IWorkflowHost<IRuntime> AsHost()
        {
            return this;
        }
    }
    
}
