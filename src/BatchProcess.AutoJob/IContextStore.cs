using System;
using System.Collections.Generic;
using System.Text;

namespace BatchProcess.AutoJob
{
    public interface IContextStore
    {
        T GetValue<T>(string key);
        void SetValue<T>(T value, string key);
    }
}
