namespace BatchProcess.AutoJob
{
    /// <summary>
    /// MessageHook handler
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IHookHandler<TKey>
        where TKey : class
    {
        string Id { get; }
        string Name { get; }
        void DoHandle(TKey sender, MessageHook message);
    }
}
