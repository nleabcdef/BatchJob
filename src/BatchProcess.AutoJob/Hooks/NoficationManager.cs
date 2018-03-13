using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BatchProcess.Shared;
using System.Threading.Tasks;

namespace BatchProcess.AutoJob
{
    /// <summary>
    /// MessageHook manager, handles all push and subscribe functionalities based on given JobId, MessageType and IHookHandler
    /// </summary>
    public sealed class NotificationManager : INotificationManager<JobId>
    {
        ConcurrentDictionary<Guid, HookNotification> _hooks { get; set; }
        const string _errorInHanlder = "Error occurred during push, sender [ JobId :{0}, Name :{1}, Type: {2} ], handler [ Id :{3}, Name :{4}].";
        const string _errorMessage = "Error on handling message [ Text :{0} ].";

        public NotificationManager()
        {
            _hooks = new ConcurrentDictionary<Guid, HookNotification>();
        }

        /// <summary>
        /// Check if the given Guid(key) is already registered
        /// </summary>
        /// <param name="id">Guid, as key</param>
        /// <returns>return true if key is registered, otherwise false.</returns>
        public bool IsRegistered(Guid id)
        {
            if (id == default(Guid)) throw new ArgumentNullException(nameof(id));

            return _hooks.ContainsKey(id);
        }

        /// <summary>
        /// Register or add subscription, based on given recieverKey, type and hook handler
        /// </summary>
        /// <param name="recieverKey">the Job id for which the handler is interested to recieve or get message notification</param>
        /// <param name="type">type of message</param>
        /// <param name="handler">the handler, is the one responsible to recieve push notification</param>
        /// <returns>Guid as key</returns>
        public Guid RegisterHook(JobId recieverKey, MessageType type, IHookHandler<JobId> handler)
        {
            if (recieverKey == default(JobId)) throw new ArgumentNullException(nameof(recieverKey));
            if (handler == default(IHookHandler<JobId>)) throw new ArgumentNullException(nameof(handler));

            var val = _hooks.Values.Where(h => h.Id.Equals(recieverKey) && h.Type == type && object.Equals(h.Handler, handler));
            if (val.Any())
                return val.FirstOrDefault().Key;

            var id = Guid.NewGuid();
            _hooks[id] = new HookNotification() { Key = id, Id = recieverKey, Type = type, Handler = handler };

            return id;
        }

        /// <summary>
        /// Pushes the notiofication or message hook to all registered handlers
        /// </summary>
        /// <param name="senderKey">sender of message hook</param>
        /// <param name="message">formatted message hook</param>
        public void PushAsync(JobId senderKey, MessageHook message)
        {
            if (senderKey == default(JobId)) throw new ArgumentNullException(nameof(senderKey));
            if (message == default(IHookHandler<JobId>)) throw new ArgumentNullException(nameof(message));

            var val = _hooks.Values.Where(h => h.Id.Equals(senderKey) && (h.Type == message.Type || h.Type == MessageType.All));
            if (val.Any())
            {
                val.AsParallel().ForAll(h =>
                {
                    new Task(() => Push(senderKey, message, h)).Start();
                });
            }
        }        

        /// <summary>
        /// Removes the Given key from message hook subscription
        /// </summary>
        /// <param name="id">registered key</param>
        public void RemoveHook(Guid id)
        {
            if (id == default(Guid))
                throw new ArgumentNullException(nameof(id));

            _hooks.TryRemove(id, out HookNotification hook);
        }

        private void Push(JobId senderKey, MessageHook message, HookNotification h)
        {
            ErrorHandle.Expect(() =>
            {
                h.Handler.DoHandle(senderKey, message);
                return true;
            },
            out bool anyError,
            string.Format(_errorInHanlder,
                senderKey.Id,
                senderKey.Name,
                message.Type,
                h.Handler.Id,
                h.Handler.Name),
            string.Format(_errorMessage,
                message.Text));
        }

        /// <summary>
        /// Dto used to manage message hooks subscription
        /// </summary>
        private class HookNotification
        {
            public Guid Key { get; set; }
            public JobId Id { get; set; }
            public MessageType Type { get; set; }
            public IHookHandler<JobId> Handler { get; set; }
        }
    }
}
