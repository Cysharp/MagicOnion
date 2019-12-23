using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MagicOnion.Server
{
    /// <summary>
    /// An interface of service locator for non-MagicOnion types.
    /// <see cref="IServiceLocator"/> doesn't provide MagicOnion related types (<see cref="IService{TSelf}"/>, <see cref="IStreamingHub{TSelf,TReceiver}"/>, formatter, etc...).
    /// </summary>
    public interface IServiceLocator
    {
        T GetService<T>();

        IServiceLocatorScope CreateScope();
    }

    public interface IServiceLocatorScope : IDisposable
    {
        IServiceLocator ServiceLocator { get; }
    }

    /// <summary>
    /// Implements a simple ServiceLocator and provides registration mechanism.
    /// <see cref="DefaultServiceLocator"/> doesn't support scoped-services. It always returns itself by <see cref="CreateScope"/>.
    /// </summary>
    public class DefaultServiceLocator : IServiceLocator, IServiceLocatorScope
    {
        public static DefaultServiceLocator Instance { get; } = new DefaultServiceLocator();

        DefaultServiceLocator()
        {

        }

        public T GetService<T>()
        {
            var f = Cache<T>.cache;
            if (f == null)
            {
                throw new InvalidOperationException("Singleton service cache is null. class:" + typeof(T).FullName);
            }

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

        public IServiceLocatorScope CreateScope() => this;

        IServiceLocator IServiceLocatorScope.ServiceLocator => this;

        static class Cache<T>
        {
            public static Func<T> cache;
        }

        public void Dispose()
        {
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
            where TServiceInterface : IServiceMarker
        {
            var instance = context.ServiceActivator.Create<TServiceBase>(context.ServiceLocator);
            instance.Context = context;
            return instance;
        }
    }
}