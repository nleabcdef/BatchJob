using BatchProcess.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcess.AutoJob
{
    /// <summary>
    /// Default IHookHandler<JobId> implementation, push the message hooks into configured logning infrastructure
    /// </summary>
    internal class HookHandler : IHookHandler<JobId>
    {
        public string Id => _id;
        public string Name => _name;

        string _id { get; set; }
        string _name { get; set; }
        const string _message = "Message recieved from Job Id: {0}.";
        const string _messageHanlder = "Message hanlded by Hook Id: {0}, Name: {1}.";

        private static string _defaultName = "Default-HookHandler";
        private ILogAdapter _logger { get; set; }

        public HookHandler() : this(null, null) { }
        public HookHandler(string id = "", string name = "")
        {
            _id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id;
            _name = string.IsNullOrWhiteSpace(name) ? _defaultName : name;
            _logger = ErrorHandle.Logger;
        }

        /// <summary>
        /// Push recieved MessageHook into configured infrastructure
        /// </summary>
        /// <param name="sender">job's indentifier is the one, who psuhed the message hook</param>
        /// <param name="message">MessageHook to be handled</param>
        public Task<bool> DoHandle(JobId sender, MessageHook message)
        {
            return new TaskFactory<bool>().StartNew(() =>
            {
                _logger.LogMessage(string.Format(_message, sender.ToString()), LogLevel.Info);
                _logger.LogMessage(message.ToString(), LogLevel.Info);
                _logger.LogMessage(string.Format(_messageHanlder, Id, Name), LogLevel.Info);
                return true;
            });
        }
    }
}