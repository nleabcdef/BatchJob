using System;
using System.Threading.Tasks;

namespace BatchProcess.AutoJob
{
    /// <summary>
    /// Manages all message hooks, with register, de register and push options
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface INotificationManager<TKey>
        where TKey : class
    {
        Guid RegisterHook(TKey recieverKey, MessageType type, IHookHandler<TKey> handler);
        bool IsRegistered(Guid id);
        void PushAsync(TKey senderKey, MessageHook message);
        void RemoveHook(Guid id);
    }
}
