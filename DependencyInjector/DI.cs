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
        public TDependency Resolve<TDependency>(string name = null) where TDependency : class
        {
            throw new NotImplementedException();
        }

        public DI(DependencyConfiguration configuration)
        {
        }
    }
}