using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Environment
{
    public static class Services
    {
        private static readonly Dictionary<Type, Type> ServicesDependencies = new Dictionary<Type, Type>();

        static readonly Dictionary<Type, object> ServicesImplementations = new Dictionary<Type, object>();

        private static readonly object servicesLock = new object();

        public static bool Available<T>() where T : class => GetImplementationType<T>() is not null;

        public static Type GetImplementationType<T>() where T : class
        {
            lock (servicesLock)
            {
                Type type = typeof(T);

                if (ServicesDependencies.ContainsKey(type))
                    return ServicesDependencies[type];

                return null;
            }
        }

        public static T GetInstance<T>() where T : class
        {
            Type type = typeof(T);

            lock (servicesLock)
            {
                if (ServicesDependencies.ContainsKey(type) == true)
                {
                    if (ServicesImplementations.ContainsKey(type) == false)
                        ServicesImplementations.Add(type, CreateInstance<T>());

                    return ServicesImplementations[type] as T;
                }
            }

            return null;
        }

        public static T CreateInstance<T>(params object[] args) where T : class
        {
            lock (servicesLock)
            {
                Type type = typeof(T);

                if (ServicesDependencies.ContainsKey(type) == true)
                    return Activator.CreateInstance(ServicesDependencies[type], args) as T;

                return null;
            }
        }

        public static void Register<T>() where T : class => Register<T, T>();

        public static void Register<TService, TImpl>() where TService : class where TImpl : class, TService
        {
            lock (servicesLock)
            {
                Type serviceType = typeof(TService);
                Type implementionType = typeof(TImpl);

                if (ServicesDependencies.ContainsKey(serviceType) == true)
                    ServicesDependencies.Remove(serviceType);

                ServicesDependencies.Add(serviceType, implementionType);
            }
        }
    }
}
