using System;
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

            IEnumerable<Implementation> impls = Configuration.GetImplementations(dependencyType);

            return (TDependency)Activator.CreateInstance(impls.First().Type);
        }

        public DI(IDependencyConfiguration configuration)
        {
            this.Configuration = configuration;
        }
    }
}