
namespace BatchProcess.AutoJob.Runtime
{
    public class SequentialRuntime : IRuntime
    {
        public RuntimeType Type => RuntimeType.Sequential;
        public IServiceRepo ServiceProvider => ServiceRepo.Instance;
    }

    public class TaskRuntime : IRuntime
    {
        public RuntimeType Type => RuntimeType.Parallel;
        public IServiceRepo ServiceProvider => ServiceRepo.Instance;
    }

}
