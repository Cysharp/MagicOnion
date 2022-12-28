namespace MagicOnion.Client.Tests;

class MockAsyncStreamReader<T> : IAsyncStreamReader<T>
{
    readonly IReadOnlyList<T> values;
    int pos;

    public MockAsyncStreamReader(IReadOnlyList<T> values)
    {
        this.values = values;
        this.pos = 0;
    }

    public async Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        await Task.Yield();
        return (values.Count > pos++);
    }

    public T Current => values[pos - 1];
}