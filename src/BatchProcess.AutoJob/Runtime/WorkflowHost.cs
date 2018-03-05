namespace BatchProcess.AutoJob.Runtime
{
    /// <summary>
    /// Hosts workflow job, with either sequential or prallel execution of jobs
    /// </summary>
    public class WorkflowHost : IWorkflowHost
    {
        private WorkflowHost() { }
        public WorkflowHost(RuntimeType runtimeType)
        {
            Type = runtimeType;
        }

        public RuntimeType Type { get; }
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
    }
}
