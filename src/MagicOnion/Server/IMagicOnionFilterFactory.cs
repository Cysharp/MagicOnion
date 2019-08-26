namespace MagicOnion.Server
{
    public interface IMagicOnionFilterFactory<T>
    {
        T CreateInstance(IServiceLocator serviceLocator);
        int Order { get; }
    }
}