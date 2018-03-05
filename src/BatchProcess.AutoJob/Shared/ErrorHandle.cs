using System;
using System.Collections.Generic;
using System.Text;

namespace BatchProcess.Shared
{
    /// <summary>
    /// Common reusable functionality, cut accross Batch job processing
    /// </summary>
    public static class ErrorHandle
    {
        private static ILogAdapter _logger;
        public static ILogAdapter Logger
        {
            get
            {
                _logger = _logger ?? ConsoleLogAdapter.Instance;
                return _logger;
            }
            set => _logger = value ?? throw new ArgumentNullException("ILogAdapter");
        }

        public static Treturn Expect<Treturn>(Func<Treturn> invoke, out bool errorHandled, string message = null, params string[] info)
        {
            return ExHandler(invoke, (ex) => { }, out errorHandled,  message, info);
        }

        public static void Expect<Tinput>(Tinput input, Action<Tinput> invoke, out bool errorHandled, string message = null, params string[] info)
        {
            ExHandler(input, invoke, (ex) => { }, out errorHandled, message, info);
        }

        public static Treturn ExpectAndThrow<Treturn>(Func<Treturn> invoke, out bool errorHandled, string message = null, params string[] info)
        {
            return ExHandler(invoke, (ex) => { throw ex; }, out errorHandled, message, info);
        }

        public static void ExpectAndThrow<Tinput>(Tinput input, Action<Tinput> invoke, out bool errorHandled, string message = null, params string[] info)
        {
            ExHandler(input, invoke, (ex) => { throw ex; }, out errorHandled, message, info);
        }

        private static Treturn ExHandler<Treturn>(Func<Treturn> invoke, Action<Exception> afterLog, out bool errorHandled, string message = null, params string[] info)
        {
            var rtn = default(Treturn);

            try
            {
                rtn = invoke();
                errorHandled = false;
            }
            catch (Exception ex)
            {
                Hanlded(afterLog, message, info, ex);
                errorHandled = true;
            }

            return rtn;
        }

        private static void ExHandler<Tinput>(Tinput input, Action<Tinput> invoke, Action<Exception> afterLog, out bool errorHandled, string message = null, params string[] info)
        {
            try
            {
                invoke(input);
                errorHandled = false;
            }
            catch (Exception ex)
            {
                Hanlded(afterLog, message, info, ex);
                errorHandled = true;
            }
        }

        private static void Hanlded(Action<Exception> afterLog, string message, string[] info, Exception ex)
        {
            LogError(message, info, ex);
            afterLog(ex);
        }

        private static void LogError(string message, string[] info, Exception ex)
        {
            Logger.LogMessage(message, LogLevel.Error);
            Logger.LogMessage(string.Join("\n", info), LogLevel.Debug);
            Logger.Log(ex);
        }

    }
}
