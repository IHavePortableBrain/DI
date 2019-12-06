using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjector.Configuration
{
    public class DependencyConfiguration : IDependencyConfiguration
    {
        public IEnumerable<Implementation> GetAllRegisteredImplementations()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Implementation> GetImplementations(Type type)
        {
            throw new NotImplementedException();
        }

        public void Register<TDependency, TImplementation>(bool isSingleton = false, string name = null)
            where TDependency : class
            where TImplementation : TDependency
        {
            Register(typeof(TDependency), typeof(TImplementation), isSingleton, name);
        }

        public void Register(Type dependency, Type implementation, bool isSingleton = false, string name = null)
        {
        }
    }
}