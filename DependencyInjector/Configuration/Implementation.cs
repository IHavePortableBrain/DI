using System;

namespace DependencyInjector.Configuration
{
    public class Implementation
    {
        public Type Type { get; private set; }

        public bool IsSingleton { get; private set; }

        public object SingletonInstance { get; set; }

        public string Name { get; private set; }

        public Implementation(Type implementationType, bool isSingleton, string name, object singletonInstance)
        {
            Type = implementationType;
            IsSingleton = isSingleton;
            Name = name;
            SingletonInstance = singletonInstance;
        }
    }
}