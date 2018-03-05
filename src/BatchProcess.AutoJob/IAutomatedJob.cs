using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BatchProcess.AutoJob
{
    public interface IAutomatedJob
    {
        JobId Id { get; }
        IJobContext Context { get; set; }
        JobResult Doable();
        Mutex mLock { get; }
    }
}
