using System;
using System.Collections.Generic;
using System.Linq;

namespace BatchProcess.Shared
{
    /// <summary>
    /// Console logger implements ILogAdapter
    /// </summary>
    public sealed class ConsoleLogAdapter : ILogAdapter
    {
        private static ConsoleLogAdapter _logger;
        public LogLevel DefaultLevel => LogLevel.Info;

        public static ILogAdapter Instance
        {
            get
            {
                if (_logger != default(ConsoleLogAdapter))
                    return _logger;

                _logger = new ConsoleLogAdapter();

                return _logger;
            }
        }

        public void LogMessage(string message)
        {
            LogMessage(message, DefaultLevel);
        }

        private ConsoleLogAdapter() { }

        public void LogMessage(string message, LogLevel level)
        {
            Console.WriteLine(string.Format("{1}: {0}", message, level.ToString()));
        }

        public void Log(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            LogMessage(error.Message, LogLevel.Error);
        }

        public void Log(BatchProcessException error)
        {
            LogMessage("BatchProcessException - Start", LogLevel.Error);

            LogMessage(error.Message, LogLevel.Error); ;
            foreach(var item in error.InnerData)
            {
                LogMessage(string.Format("message :{0}, type: {1}.", item.Item1, item.Item2), LogLevel.Error);
            }

            LogMessage("BatchProcessException - End", LogLevel.Error);
        }
    }
}
