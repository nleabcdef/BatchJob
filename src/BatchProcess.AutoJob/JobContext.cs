using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BatchProcess.AutoJob
{
    /// <summary>
    /// Thread safe implementation of IJobContext, enables to share data/object between Automated/Workflow job processing
    /// </summary>
    public class JobContext : IJobContext
    {
        public JobId ParentJobId { get; protected set; }
        public IReadOnlyList<JobId> ProcessedJobs => _processedJobs.ToList().AsReadOnly();

        protected ConcurrentBag<JobId> _processedJobs { get; set; }
        protected ConcurrentDictionary<string, object> _values { get; set; }
        protected ConcurrentDictionary<string, Type> _types { get; set; }

        /// <summary>
        /// get value from context store
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">key to find</param>
        /// <returns></returns>
        public T GetValue<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");

            T val = default(T);

            if (_values.ContainsKey(key))
            {
                if (_values[key] == null)
                    return val;

                val = (T)_values[key];
            }
            return val;
        }

        /// <summary>
        /// add or update the value with given key to context store
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">value to be stored</param>
        /// <param name="key">key to find</param>
        public void SetValue<T>(T value, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
                    
            _values[key] = value;
            _types[key] = value == null? typeof(Nullable) : value.GetType();

        }

        /// <summary>
        /// keep track of jobs, processed this jobcontext.
        /// </summary>
        /// <param name="id"></param>
        public void AddToProcessed(JobId id)
        {
            if (id == default(JobId))
                throw new ArgumentNullException("id");

            _processedJobs.Add(id);
        }

        private JobContext() { }
        public JobContext(JobId parentId)
        {
            ParentJobId = parentId ?? throw new ArgumentNullException("parentId");

            _processedJobs = new ConcurrentBag<JobId>();
            _values = new ConcurrentDictionary<string, object>();
            _types = new ConcurrentDictionary<string, Type>();
        }
    }
}
