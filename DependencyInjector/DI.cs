using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DependencyInjector.Configuration;

namespace DependencyInjector
{
    public class DI : IDI
    {
        private readonly IDependencyConfiguration Configuration;
        private readonly ConcurrentDictionary<int, Stack<Type>> recursionControlStackByThreadId = new ConcurrentDictionary<int, Stack<Type>>();

        public TDependency Resolve<TDependency>(string name = null) where TDependency : class
        {
            Type dependencyType = typeof(TDependency);

            return (TDependency)Resolve(dependencyType, name);
        }

        private object Resolve(Type dependency, string name = null)
        {
            Implementation[] impls;
            object result = null;

            int currThreadId = Thread.CurrentThread.ManagedThreadId;
            if (!recursionControlStackByThreadId
                .TryGetValue(currThreadId, out Stack<Type> atResolvingTypes))
            {
                atResolvingTypes = new Stack<Type>();

                if (!recursionControlStackByThreadId.TryAdd(currThreadId, atResolvingTypes))
                    throw new ApplicationException();
            }
            else
            {
                //check recursion dependency
                if (atResolvingTypes.Contains(dependency))
                {
                    //try create it by constroctor
                    result = CreateByConstructor(dependency);
                    if (result != null)
                        return result;

                    throw new ArgumentException("Depedencies are recursive.");
                }
            }
            atResolvingTypes.Push(dependency);

            //TODO: rewrite with push type to stack at the begining, and pop rigth before return. To get rid of stackoverflow
            if (dependency.IsPrimitive
                || !dependency.IsClass && !dependency.IsInterface)//isenum, isStruct
                result = Activator.CreateInstance(dependency);
            else
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
                result = implInstances;
            }
            else
            {
                impls = Configuration.GetImplementations(dependency)?.ToArray();
                if (impls != null)
                {
                    result = Resolve(impls.First().Type);
                }
                else //no implementation for that dependency, try make its instance
                    result = CreateByConstructor(dependency);
            }

            atResolvingTypes.Pop();
            return result;
        }

        private object CreateByConstructor(Type type)
        {
            object result = null;

            ConstructorInfo[] cis = type.GetConstructors();
            foreach (var ci in cis)
            {
                ParameterInfo[] pis = ci.GetParameters();
                List<object> parameters = new List<object>();

                try
                {
                    foreach (var pi in pis)
                        parameters.Add(Resolve(pi.ParameterType));

                    result = Activator.CreateInstance(type, parameters.ToArray());

                    if (result != null)
                        return result;
                }
                finally
                {
                    parameters.Clear();
                }
            }

            return result;
        }

        public DI(IDependencyConfiguration configuration)
        {
            this.Configuration = configuration;
        }
    }
}