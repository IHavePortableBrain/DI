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

        //dont, change return value
        internal IEnumerable<Implementation> GetAllRegisteredImplementations()
        {
            throw new NotImplementedException();
        }

        //use it to set singleton instance for implementation
        public IEnumerable<Implementation> GetImplementations(Type type)
        {
            ImplsByDependencyType.TryGetValue(type, out List<Implementation> result);
            return result;
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

        internal void ValidateRegistration(Type dependency, Type implementation, bool isSingleton = false, string name = null)
        {
            if (!dependency.IsAssignableFrom(implementation)
                    && !(dependency.IsGenericTypeDefinition && implementation.IsGenericTypeDefinition
                        && IsAssignableFromAsOpenGeneric(dependency, implementation))
                    )
                throw new ArgumentException("Invalid dependency registration types");

            if (!dependency.IsClass && !dependency.IsInterface
                || implementation.IsAbstract)
                throw new ArgumentException("Invalid dependency registration types");
        }

        public bool IsAssignableFromAsOpenGeneric(Type type, Type c)
        {
            if (!type.IsGenericTypeDefinition || !c.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Specified types should be generic");
            }

            Type comparedType, baseType;

            Queue<Type> baseTypes = new Queue<Type>();
            baseTypes.Enqueue(c);

            bool result;

            do
            {
                comparedType = baseTypes.Dequeue();
                baseType = comparedType.BaseType;
                if ((baseType != null) && (baseType.IsGenericType || baseType.IsGenericTypeDefinition))
                {
                    baseTypes.Enqueue(baseType.GetGenericTypeDefinition());
                }
                foreach (Type baseInterface in comparedType.GetInterfaces()
                    .Where((intf) => intf.IsGenericType || intf.IsGenericTypeDefinition))
                {
                    baseTypes.Enqueue(baseInterface.GetGenericTypeDefinition());
                }
                result = comparedType == type;
            } while (!result && (baseTypes.Count > 0));

            return result;
        }
    }
}