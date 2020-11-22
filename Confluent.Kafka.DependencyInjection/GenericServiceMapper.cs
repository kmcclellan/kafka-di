using System;
using System.Collections.Generic;

namespace Confluent.Kafka.DependencyInjection
{
    class GenericServiceMapper<T> : IServiceProvider
    {
        readonly IServiceProvider provider;
        readonly IDictionary<Type, Type> mappings;

        public GenericServiceMapper(IServiceProvider provider, IDictionary<Type, Type> mappings)
        {
            this.provider = provider;
            this.mappings = mappings;
        }

        // Prepends T to generic type arguments for mapped types.
        public object? GetService(Type serviceType)
        {
            var key = serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : serviceType;
            if (mappings.TryGetValue(key, out var implementationType))
            {
                var serviceArgs = serviceType.GetGenericArguments();
                var implementationArgs = new Type[serviceArgs.Length + 1];
                implementationArgs[0] = typeof(T);
                serviceArgs.CopyTo(implementationArgs, 1);

                serviceType = implementationType.MakeGenericType(implementationArgs);
            }

            return provider.GetService(serviceType);
        }
    }
}