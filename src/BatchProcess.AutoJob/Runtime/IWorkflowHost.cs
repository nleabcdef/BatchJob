using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcess.AutoJob.Runtime
{
    /// <summary>
    /// Hosts the job execution
    /// </summary>
    /// <typeparam name="TRuntime"></typeparam>
    public interface IWorkflowHost<out TRuntime>
        where TRuntime : IRuntime
    {
        TRuntime Runtime { get; }
        IWorkflowHost<IRuntime> AsHost();
        IWorkflowRunner Start(IWorkflowJob workflow);
    }
}
