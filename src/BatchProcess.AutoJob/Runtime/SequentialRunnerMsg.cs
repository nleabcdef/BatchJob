using BatchProcess.AutoJob.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace BatchProcess.AutoJob.Runtime
{
    /// <summary>
    /// defines the error/message constants used in Sequential Runner
    /// </summary>
    public partial class SequentialRunner : IWorkflowHost, IWorkflowRunner
    {
        private const string _msgError = "Error occurred in Retry Job.";
        private const string _msgJobError = "Error < Retry Job of id: {0}, name: {1}>.";
        private const string _msgWError = "Error < WorkflowJob of id: {0}, name: {1}>.";
        private const string _msgContextError = "coudl not set, Context. mLock handle is not acquired.";
        private const string _msgNoWorkflow = "no workflow job are configured.";
        private const string _msgAbortError = "Workflow Job aborted, Error < Job of id: {0}, name: {1}>.";
        private const string _msgAborted = "Job aborted, OnFailure set to  WhenFailure.StopOrExitJob.";
        private const string _msgCancel = "Job processing has been stopped, On Stop command.";
    }
}
