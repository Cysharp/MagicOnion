using System;
using System.Linq;
using System.Linq.Expressions;

namespace MagicOnion.Server
{
    public interface IServiceActivator
    {
        T Create<T>(IServiceLocator serviceLocator);
    }

    public class DefaultServiceActivator : IServiceActivator
    {
        public static IServiceActivator Instance { get; } = new DefaultServiceActivator();

        private DefaultServiceActivator()
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