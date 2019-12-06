using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjector
{
    public interface IDI
    {
        TDependency Resolve<TDependency>(string name = null)
            where TDependency : class;
    }
}