namespace MagicOnion.Client.Tests;

class MockClientStreamWriter<T> : IClientStreamWriter<T>
{
    public List<T> Written { get; } = new List<T>();
    public bool Completed { get; private set; }

    public Task WriteAsync(T message)
    {
        Written.Add(message);
        return Task.CompletedTask;
    }

    public WriteOptions? WriteOptions { get; set; } = WriteOptions.Default;

    public Task CompleteAsync()
    {
        Completed = true;
        return Task.CompletedTask;
    }
}
