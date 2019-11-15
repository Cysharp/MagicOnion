using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MagicOnion.Server
{
    public interface IServiceLocator
    {
        T GetService<T>();
        void Register<T>(); // transient
        void Register<T>(T singleton);
    }

    public class DefaultServiceLocator : IServiceLocator
    {
        public static readonly IServiceLocator Instance = new DefaultServiceLocator();

        DefaultServiceLocator()
        {

        }

        public T GetService<T>()
        {
            return Cache<T>.cache();
        }

        public void Register<T>(T singleton)
        {
            Cache<T>.cache = () => singleton;
        }

        public void Register<T>()
        {
            if (!typeof(T).GetConstructors().Any(x => x.GetParameters().Length == 0))
            {
                throw new InvalidOperationException(string.Format("Type needs parameterless constructor, class:{0}", typeof(T).FullName));
            }

            var factory = Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
            Cache<T>.cache = factory;
        }

        static class Cache<T>
        {
            public static Func<T> cache;
        }
    }

    internal static class ServiceLocatorHelper
    {
        static readonly MethodInfo serviceLocatorGetServiceT = typeof(IServiceLocator).GetMethod("GetService");

        internal static object GetService(this IServiceLocator serviceLocator, Type t)
        {
            return serviceLocatorGetServiceT.MakeGenericMethod(t).Invoke(serviceLocator, null);
        }

        internal static TServiceBase CreateService<TServiceBase, TServiceInterface>(ServiceContext context)
            where TServiceBase : ServiceBase<TServiceInterface>
            where TServiceInterface : IService<TServiceInterface>
        {
            var instance = context.ServiceLocator.GetService<TServiceBase>();
            instance.Context = context;
            return instance;
        }
    }
}