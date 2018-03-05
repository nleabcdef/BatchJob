using System;
using System.Collections.Generic;
using System.Text;

namespace BatchProcess.Shared
{
    /// <summary>
    /// Adapter to interchange Logging functionality
    /// </summary>
    public interface ILogAdapter
    {
        LogLevel DefaultLevel { get; }
        void LogMessage(string message);
        void LogMessage(string message, LogLevel level);

        void Log(Exception error);
        void Log(BatchProcessException error);
    }
}
