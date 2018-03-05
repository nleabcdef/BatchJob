using System;
using System.Collections.Generic;
using System.Text;

namespace BatchProcess.Shared
{
    /// <summary>
    /// Custom exception handles the error details for Batch job processing.
    /// </summary>
    public class BatchProcessException : Exception
    {
        public ErrorCategory Category { get; protected set; }
        protected List<Tuple<string, ErrorCategory>> _innerData { get; set; }
        public IReadOnlyList<Tuple<string, ErrorCategory>> InnerData => _innerData.AsReadOnly();

        private BatchProcessException() { }

        public BatchProcessException(string message, Exception innerException)
        : base(message, innerException)
        {
            _innerData = new List<Tuple<string, ErrorCategory>>();
        }

        public BatchProcessException(string message, Exception innerException, List<Tuple<string, ErrorCategory>> innerData)
        : this(message, innerException) => _innerData = innerData ?? throw new ArgumentNullException("innerData");
    }

    public enum ErrorCategory
    {
        InvalidInput,
        InvalidJob,
        RetryExceeded,
        Others
    }

}
