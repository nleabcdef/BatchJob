using System;
using System.Text;

namespace BatchProcess.AutoJob
{
    /// <summary>
    /// ServiceProvider repository to resolve and get default/internal dependencies
    /// </summary>
    public interface IServiceRepo : IServiceProvider
    {
        IServiceProvider Provider { get; }
        string Name { get; }
        T GetServiceOf<T>();
    }
}
