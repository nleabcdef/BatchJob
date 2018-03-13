
namespace BatchProcess.AutoJob.Runtime
{
    /// <summary>
    /// Runtime services for job execution environment
    /// </summary>
    public interface IRuntime
    {
        IServiceRepo ServiceProvider { get; }
        RuntimeType Type { get; }
    }
}