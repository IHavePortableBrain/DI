﻿using System;
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

            var result = Resolve(dependencyType, name);

            return (TDependency)result;//will cause cast error for generic dependencies wich are not covariant to their generic arguments
        }

        internal object Resolve(Type dependency, string name = null)
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
                    implInstances[i] = impls[i].ResolveOrReturnSingletonInstance(this, name);
                }
                result = implInstances;
            }
            else
            {
                impls = Configuration.GetImplementations(dependency)?.ToArray();
                if (impls == null && dependency.IsGenericType)//handle search for open generic types impls as well
                    impls = Configuration.GetImplementations(dependency.GetGenericTypeDefinition())?.ToArray();
                if (impls != null)
                {
                    Implementation implToUse = impls.First();
                    if (name != null)
                        implToUse = Array.Find(impls, impl => impl.Name == name);

                    result = implToUse?.ResolveOrReturnSingletonInstance(this, name); //TODO: resolve dependency on GenericType of impl of open generic dependency
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

            ResolveIfContainsGenericParameter(ref type);

            ConstructorInfo[] cis = type.GetConstructors();
            foreach (var ci in cis)
            {
                ParameterInfo[] pis = ci.GetParameters();
                List<object> parameters = new List<object>();

                try
                {
                    foreach (var pi in pis)
                    {
                        string dependencyExplictName = (string)pi.GetCustomAttribute<DependencyKeyAttribute>()?.Name;
                        parameters.Add(Resolve(pi.ParameterType, dependencyExplictName));
                    }

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

        private void ResolveIfContainsGenericParameter(ref Type type)
        {
            if (type.ContainsGenericParameters)
            {
                Type[] toResolve = type.GetGenericArguments();

                Type[] genericParameters = toResolve.Select(dep =>
                {
                    var impls = Configuration.GetImplementations(dep.BaseType)?.ToArray();
                    return impls != null ? impls.First().Type : dep.BaseType;
                })
                .ToArray();

                type = type.MakeGenericType(genericParameters);
            }
        }

        public DI(IDependencyConfiguration configuration)
        {
            this.Configuration = configuration;
        }
    }
}