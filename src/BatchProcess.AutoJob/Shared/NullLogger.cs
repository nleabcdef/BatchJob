using System;
using System.Collections.Generic;
using System.Text;

namespace BatchProcess.Shared
{
    /// <summary>
    /// null logger, to disable default console logging behaviour.
    /// </summary>
    public sealed class NullLogger : ILogAdapter
    {
        public LogLevel DefaultLevel => LogLevel.Info;

        public void Log(Exception error) { }

        public void Log(BatchProcessException error) { }

        public void LogMessage(string message) { }

        public void LogMessage(string message, LogLevel level) { }

        private static NullLogger _logger {get; set;}
        private NullLogger() { }

        public static NullLogger Instance
        {
            get
            {
                _logger = _logger ?? new NullLogger();

                return _logger;
            }
        }
    }
}
