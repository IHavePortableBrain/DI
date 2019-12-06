using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjector.Configuration;

namespace DependencyInjector
{
    public class DI : IDI
    {
        private readonly IDependencyConfiguration Configuration;

        public TDependency Resolve<TDependency>(string name = null) where TDependency : class
        {
            Type dependencyType = typeof(TDependency);

            return (TDependency)Resolve(dependencyType, name);
        }

        private object Resolve(Type dependency, string name)
        {
            Implementation[] impls;

            if (typeof(IEnumerable).IsAssignableFrom(dependency))
            {
                Type dependencyType = dependency.GetGenericArguments()[0];
                impls = Configuration.GetImplementations(dependencyType).ToArray();

                //create array of Ts (when dependency is IEnumerable<T>)
                var implInstances = (object[])Activator.CreateInstance(dependencyType.MakeArrayType(), new object[] { impls.Count() });
                for (int i = 0; i < impls.Count(); i++)
                {
                    implInstances[i] = Resolve(impls[i].Type, name);
                }
                return implInstances;
            }

            impls = Configuration.GetImplementations(dependency)?.ToArray();
            if (impls != null)
            {
                return Activator.CreateInstance(impls.First().Type);
            }

            //no implementation for that dependency, try make it instance
            return Activator.CreateInstance(dependency);
        }

        public DI(IDependencyConfiguration configuration)
        {
            this.Configuration = configuration;
        }
    }
}