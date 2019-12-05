using System;
using System.Linq;
using System.Linq.Expressions;

namespace MagicOnion.Server
{
    /// <summary>
    /// An interface for MagicOnion services (<see cref="IService{TSelf}"/>, <see cref="IStreamingHub{TSelf,TReceiver}"/>) activator.
    /// </summary>
    public interface IMagicOnionServiceActivator
    {
        /// <summary>
        /// Create an MagicOnion service instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceLocator"></param>
        /// <returns></returns>
        T Create<T>(IServiceLocator serviceLocator);
    }

    /// <summary>
    /// Provides MagicOnion service activation default-mechanism. This activator doesn't use <see cref="IServiceLocator"/> for constructor.
    /// </summary>
    public class DefaultMagicOnionServiceActivator : IMagicOnionServiceActivator
    {
        public static IMagicOnionServiceActivator Instance { get; } = new DefaultMagicOnionServiceActivator();

        private DefaultMagicOnionServiceActivator()
        { }

        public T Create<T>(IServiceLocator serviceLocator)
        {
            return Cache<T>.cache();
        }

        static class Cache<T>
        {
            public static Func<T> cache;

            static Cache()
            {
                if (!typeof(T).GetConstructors().Any(x => x.GetParameters().Length == 0))
                {
                    throw new InvalidOperationException(string.Format("Type needs parameterless constructor, class:{0}", typeof(T).FullName));
                }

                var factory = Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
                cache = factory;
            }
        }
    }
}