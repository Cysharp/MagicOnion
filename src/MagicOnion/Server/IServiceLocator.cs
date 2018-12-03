namespace MagicOnion.Server
{
    public interface IServiceLocator
    {
        T GetService<T>();
        void Register<T>(T service);
        void Unregister<T>();
    }

    public class DefaultServiceLocator : IServiceLocator
    {
        public static readonly DefaultServiceLocator Instance = new DefaultServiceLocator();

        DefaultServiceLocator()
        {

        }

        public T GetService<T>()
        {
            return Cache<T>.cache;
        }

        public void Register<T>(T service)
        {
            Cache<T>.cache = service;
        }

        public void Unregister<T>()
        {
            Cache<T>.cache = default(T);
        }

        static class Cache<T>
        {
            public static T cache;
        }
    }
}