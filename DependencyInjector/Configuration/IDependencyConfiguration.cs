using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjector.Configuration
{
    public interface IDependencyConfiguration
    {
        void Register<TDependency, TImplementation>(bool isSingleton = false, string name = null)
            where TDependency : class
            where TImplementation : TDependency;

        //mainly for open generic registration
        void Register(Type dependency, Type implementation, bool isSingleton = false, string name = null);

        IEnumerable<Implementation> GetImplementations(Type type);
    }
}