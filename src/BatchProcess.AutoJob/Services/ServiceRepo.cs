using BatchProcess.AutoJob.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BatchProcess.AutoJob
{
    /// <summary>
    /// ServiceProvider repository to resolve and get default/internal dependencies
    /// </summary>
    public sealed class ServiceRepo : IServiceRepo
    {
        public string Name => _name;
        public IServiceProvider Provider => this;

        string _name { get; set; }

        /// <summary>
        /// stores the Singleton objects
        /// </summary>
        ConcurrentDictionary<Type, object> _mappings { get; set; }

        /// <summary>
        /// stores the type mapping
        /// </summary>
        ConcurrentDictionary<Type, Type> _typeMappings { get; set; }

        private static IServiceProvider _provider { get; set; }
        private static IServiceRepo _repo { get; set; }

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static IServiceRepo Instance
        {
            get
            {
                _repo = _repo ?? new ServiceRepo("Default-ServiceRepo");
                return _repo;
            }
        }

        /// <summary>
        /// Updates the custom IServiceProvider, to override the existing/default IServiceRepo implementation
        /// </summary>
        /// <param name="provider">IServiceProvider implementation</param>
        /// <returns>returns the IServiceRepo, after update</returns>
        public static IServiceRepo CreateOrRepo(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            return Instance;
        }

        /// <summary>
        /// returns the service for given Type, returns null if not found in service catalog
        /// </summary>
        /// <param name="serviceType">type to find</param>
        /// <returns>instance, returns null if not found in service catalog</returns>
        public object GetService(Type serviceType)
        {
            if (serviceType == default(Type)) throw new ArgumentNullException(nameof(serviceType));

            return _provider?.GetService(serviceType) ?? GetFromRepo(serviceType);
        }

        /// <summary>
        ///  returns the service for given Type, returns null if not found in service catalog
        /// </summary>
        /// <typeparam name="T">type to find</typeparam>
        /// <returns>instance, returns null if not found in service catalog</returns>
        public T GetServiceOf<T>()
        {
            Type serviceType = typeof(T);
            var result = GetService(serviceType);

            return result == null ? default(T) : (T)result;
        }

        /// <summary>
        /// Private ctor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="provider"></param>
        private ServiceRepo(string name, IServiceProvider provider = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            _name = name;
            _provider = provider;
            Init(); 
        }
        
        private void Init()
        {
            _mappings = new ConcurrentDictionary<Type, object>();
            _typeMappings = new ConcurrentDictionary<Type, Type>(GetDefaultTypes());

            _mappings[typeof(INotificationManager<JobId>)] = new NotificationManager();
        
        }

        /// <summary>
        /// Default type mapping setup.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<Type, Type>> GetDefaultTypes()
        {
            return new List<KeyValuePair<Type, Type>>()
            {
                new KeyValuePair<Type, Type>(typeof(IHookHandler<JobId>), typeof(HookHandler)),
                new KeyValuePair<Type, Type>(typeof(INotificationManager<JobId>), typeof(NotificationManager)),
                new KeyValuePair<Type, Type>(typeof(SequentialRuntime), typeof(SequentialRuntime)),
                new KeyValuePair<Type, Type>(typeof(TaskRuntime), typeof(TaskRuntime)),
                new KeyValuePair<Type, Type>(typeof(IWorkflowJob), typeof(WorkflowJob)), //todo
                new KeyValuePair<Type, Type>(typeof(IWorkflowThread<JobResult>), typeof(WorkflowThread)),
                new KeyValuePair<Type, Type>(typeof(IWorkflowHost<SequentialRuntime>), typeof(SequentialRunner)), 
                new KeyValuePair<Type, Type>(typeof(IWorkflowHost<TaskRuntime>), typeof(TaskRunner)) 
            };
        }
        
        private object GetFromRepo(Type serviceType)
        {
            if (_mappings.ContainsKey(serviceType))
                return _mappings[serviceType];
            
            if(_typeMappings.ContainsKey(serviceType))
            {
                // need to implement better object construction, to handle constructor params and dependencies
                return Activator.CreateInstance(_typeMappings[serviceType]); 
            }

            return null;
        }

        
    }
}
