using System.Collections.Generic;
using System.Text;

namespace BatchProcess.AutoJob
{
    public class JobResult
    {
        public JobStatus Status { get; protected set; }
        public AutoJobException Error { get; protected set; }
        private JobResult() { }
        public JobResult(JobStatus status, AutoJobException error = null)
        {
            Status = status;
            Error = error;
        }
    }
}
