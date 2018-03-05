namespace BatchProcess.AutoJob.Runtime
{
    /// <summary>
    /// defines the constant used during parallel execution
    /// </summary>
    public partial class TaskRunner : IWorkflowHost, IWorkflowRunner
    {
        private const string _msgError = "Error occurred in Workflow job.";
        private const string _msgNoWorkflow = "no workflow job are configured.";
        private const string _msgJobError = "Error < Retry Job of id: {0}, name: {1}>.";
        private const string _msgWError = "Error < WorkflowJob of id: {0}, name: {1}>.";
    }
}
