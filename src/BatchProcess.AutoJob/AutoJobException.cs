using System;

namespace BatchProcess.AutoJob
{
    /// <summary>
    /// Custom exception handles the error details for Automated job processing.
    /// </summary>
    [Serializable]
    public class AutoJobException : Exception
    {
        protected const string _defaultMessage = "Error occurred during AutoJob execution.";
        protected const string _defaultString = "Message: {0}, JobDetail : [ Id : {1}, Name : {2}].";
        public JobId JobDetail { get; protected set; }
        public override string Message => base.Message;

        private AutoJobException() { }

        public AutoJobException(JobId jobDetail, Exception innerException = null, string message = null)
            : base(string.IsNullOrWhiteSpace(message) ? _defaultMessage : message, innerException) => JobDetail = jobDetail ?? throw new ArgumentNullException("jobDetail");

        public override string ToString()
        {
            return string.Format(_defaultString, Message, JobDetail.Id, JobDetail.Name);
        }
    }
}
