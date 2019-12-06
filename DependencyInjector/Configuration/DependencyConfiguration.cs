using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjector.Configuration
{
    public class DependencyConfiguration : IDependencyConfiguration
    {
        private readonly ConcurrentDictionary<Type, List<Implementation>> ImplsByDependencyType = new ConcurrentDictionary<Type, List<Implementation>>();

        public IEnumerable<Implementation> GetAllRegisteredImplementations()
        {
            throw new NotImplementedException();
        }

        //use it to set singleton instance for implementation
        public IEnumerable<Implementation> GetImplementations(Type type)
        {
            return ImplsByDependencyType[type];
        }

        public void Register<TDependency, TImplementation>(bool isSingleton = false, string name = null)
            where TDependency : class
            where TImplementation : TDependency
        {
            Register(typeof(TDependency), typeof(TImplementation), isSingleton, name);
        }

        public void Register(Type dependency, Type implementation, bool isSingleton = false, string name = null)
        {
            ValidateRegistration(dependency, implementation, isSingleton, name);

            List<Implementation> impls;

            if (!ImplsByDependencyType.TryGetValue(dependency, out impls))
            {
                impls = new List<Implementation>();
                ImplsByDependencyType[dependency] = impls;
            }

            if (name != null)
                impls.RemoveAll(impl => impl.Name == name);

            impls.Add(new Implementation(implementation, isSingleton, name, null));
        }

        private void ValidateRegistration(Type dependency, Type implementation, bool isSingleton, string name)
        {
        }
    }
}