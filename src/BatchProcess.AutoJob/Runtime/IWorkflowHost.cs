using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcess.AutoJob.Runtime
{
    public interface IWorkflowHost
    {
        RuntimeType Type { get; }
        IWorkflowRunner Start(IWorkflowJob workflow);
    }
}
