using System;
using System.Collections.Generic;
using System.Text;

namespace BatchProcess.AutoJob
{
    public enum WhenFailure
    {
        ContinueOn = 1,
        StopOrExitJob
    }
}
