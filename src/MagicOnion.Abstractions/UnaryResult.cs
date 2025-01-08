using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.CompilerServices; // require this using in AsyncMethodBuilder
using System.Runtime.CompilerServices;
using MessagePack;

namespace MagicOnion;

/// <summary>
/// Represents the result of a Unary call that wraps AsyncUnaryCall as Task-like.
/// </summary>
[AsyncMethodBuilder(typeof(AsyncUnaryResultMethodBuilder))]
public readonly struct UnaryResult
{
    readonly object? value;

    [Obsolete("Use default(UnaryResult) instead.")]
    public UnaryResult(Nil nil)
    {
        this.value = default;
    }

    [Obsolete("Use UnaryResult(Task) instead.")]
    public UnaryResult(Task<Nil> rawTaskValue) : this((Task)rawTaskValue)
    { }

    public UnaryResult(Task<IResponseContext<Nil>> response)
    {
        this.value = response ?? throw new ArgumentNullException(nameof(response));
    }

    public UnaryResult(Task rawTaskValue)
    {
        this.value = rawTaskValue ?? throw new ArgumentNullException(nameof(rawTaskValue));
    }

    internal bool HasRawValue
        => this.value is null ||
           this.value is Task { IsCompleted: true, IsFaulted: false } and not Task<IResponseContext<Nil>>;

    /// <summary>
    /// Asynchronous call result.
    /// </summary>
    public Task ResponseAsync
    {
        get
        {
            // This result has a raw Task value.
            if (value?.GetType() == typeof(Task) || value is Task<Nil>)
            {
                return (Task)value; // Task or Task<Nil>
            }

            // This result has a response Task value.
            if (value is Task<IResponseContext<Nil>>)
            {
                return UnwrapResponse();
            }

            // If the UnaryResult has no raw-value and no response, it is the default value of UnaryResult.
            // So, we will return the default value of TResponse as Task.
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Asynchronous access to response headers.
    /// </summary>
    public Task<Metadata> ResponseHeadersAsync => UnwrapResponseHeaders();

    Task<IResponseContext<Nil>> GetRequiredResponse()
        => this.value as Task<IResponseContext<Nil>> ?? throw new InvalidOperationException("UnaryResult has no response.");

    async Task UnwrapResponse()
    {
        var ctx = await GetRequiredResponse().ConfigureAwait(false);
        await ctx.ResponseAsync.ConfigureAwait(false);
    }

    async Task<Metadata> UnwrapResponseHeaders()
    {
        var ctx = await GetRequiredResponse().ConfigureAwait(false);
        return await ctx.ResponseHeadersAsync.ConfigureAwait(false);
    }

    IResponseContext TryUnwrap()
    {
        var response = GetRequiredResponse();
        if (!response.IsCompleted)
        {
            throw new InvalidOperationException("UnaryResult request is not yet completed, please await before call this.");
        }

        return response.Result;
    }

    /// <summary>
    /// Allows awaiting this object directly.
    /// </summary>
    public TaskAwaiter GetAwaiter()
        => ResponseAsync.GetAwaiter();

    /// <summary>
    /// Configures an awaiter used to await this object.
    /// </summary>
    public ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext)
        => ResponseAsync.ConfigureAwait(continueOnCapturedContext);

    /// <summary>
    /// Gets the call status if the call has already finished.
    /// Throws InvalidOperationException otherwise.
    /// </summary>
    public Status GetStatus()
        => TryUnwrap().GetStatus();

    /// <summary>
    /// Gets the call trailing metadata if the call has already finished.
    /// Throws InvalidOperationException otherwise.
    /// </summary>
    public Metadata GetTrailers()
        => TryUnwrap().GetTrailers();

    /// <summary>
    /// Provides means to cleanup after the call.
    /// If the call has already finished normally (request stream has been completed and call result has been received), doesn't do anything.
    /// Otherwise, requests cancellation of the call which should terminate all pending async operations associated with the call.
    /// As a result, all resources being used by the call should be released eventually.
    /// </summary>
    /// <remarks>
    /// Normally, there is no need for you to dispose the call unless you want to utilize the
    /// "Cancel" semantics of invoking <c>Dispose</c>.
    /// </remarks>
    public void Dispose()
    {
        if (value is Task<IResponseContext<Nil>> responseTask)
        {
            if (!responseTask.IsCompleted)
            {
                UnwrapDispose();
            }
            else
            {
                responseTask.Result.Dispose();
            }
        }
    }
        
    async void UnwrapDispose()
    {
        try
        {
            var ctx = await GetRequiredResponse().ConfigureAwait(false);
            ctx.Dispose();
        }
        catch
        {
        }
    }

    /// <summary>
    /// Gets a completed <see cref="T:MagicOnion.UnaryResult" /> with the void result.
    /// </summary>
    public static UnaryResult CompletedResult => default;

    /// <summary>
    /// Creates a <see cref="T:MagicOnion.UnaryResult`1" /> with the specified result.
    /// </summary>
    public static UnaryResult<T> FromResult<T>(T value)
        => new UnaryResult<T>(value);

    /// <summary>
    /// Creates a <see cref="T:MagicOnion.UnaryResult`1" /> with the specified result task.
    /// </summary>
    public static UnaryResult<T> FromResult<T>(Task<T> task)
        => new UnaryResult<T>(task);

    /// <summary>
    /// Gets the result that contains <see cref="F:MessagePack.Nil.Default"/> as the result value.
    /// </summary>
    [Obsolete("Use `UnaryResult` or `UnaryResult.FromResult(Nil.Default)` instead.")]
    public static UnaryResult<Nil> Nil
        => new UnaryResult<Nil>(MessagePack.Nil.Default);
}


/// <summary>
/// Represents the result of a Unary call that wraps AsyncUnaryCall as Task-like.
/// </summary>
[AsyncMethodBuilder(typeof(AsyncUnaryResultMethodBuilder<>))]
public readonly struct UnaryResult<TResponse>
{
    readonly TResponse? rawValue;
    readonly object? valueTask;

    public UnaryResult(TResponse rawValue)
    {
        this.rawValue = rawValue;
    }

    public UnaryResult(Task<TResponse> rawTaskValue)
    {
        this.valueTask = rawTaskValue ?? throw new ArgumentNullException(nameof(rawTaskValue));
    }

    public UnaryResult(Task<IResponseContext<TResponse>> response)
    {
        this.valueTask = response ?? throw new ArgumentNullException(nameof(response));
    }

    internal bool HasRawValue
        => this.valueTask is null ||
           this.valueTask is Task<TResponse> { IsCompleted: true, IsFaulted: false };

    /// <summary>
    /// Asynchronous call result.
    /// </summary>
    public Task<TResponse> ResponseAsync
    {
        get
        {
            // This result has a raw value.
            if (this.valueTask is null)
            {
                return Task.FromResult(rawValue!);
            }

            // This result has a raw Task value.
            if (this.valueTask is Task<TResponse> t)
            {
                return t;
            }

            // This result has a response Task value.
            if (valueTask is Task<IResponseContext<TResponse>>)
            {
                return UnwrapResponse();
            }

            // If the UnaryResult has no raw-value and no response, it is the default value of UnaryResult.
            // So, we will return the default value of TResponse as Task.
            return Task.FromResult(default(TResponse)!);
        }
    }

    /// <summary>
    /// Asynchronous access to response headers.
    /// </summary>
    public Task<Metadata> ResponseHeadersAsync => UnwrapResponseHeaders();

    Task<IResponseContext<TResponse>> GetRequiredResponse()
        => (valueTask as Task<IResponseContext<TResponse>>) ?? throw new InvalidOperationException("UnaryResult has no response.");

    async Task<TResponse> UnwrapResponse()
    {
        var ctx = await GetRequiredResponse().ConfigureAwait(false);
        return await ctx.ResponseAsync.ConfigureAwait(false);
    }

    async Task<Metadata> UnwrapResponseHeaders()
    {
        var ctx = await GetRequiredResponse().ConfigureAwait(false);
        return await ctx.ResponseHeadersAsync.ConfigureAwait(false);
    }

    async void UnwrapDispose()
    {
        try
        {
            var ctx = await GetRequiredResponse().ConfigureAwait(false);
            ctx.Dispose();
        }
        catch
        {
        }
    }

    IResponseContext<TResponse> TryUnwrap()
    {
        var response = GetRequiredResponse();
        if (!response.IsCompleted)
        {
            throw new InvalidOperationException("UnaryResult request is not yet completed, please await before call this.");
        }

        return response.Result;
    }

    /// <summary>
    /// Allows awaiting this object directly.
    /// </summary>
    public TaskAwaiter<TResponse> GetAwaiter()
        => ResponseAsync.GetAwaiter();

    /// <summary>
    /// Configures an awaiter used to await this object.
    /// </summary>
    public ConfiguredTaskAwaitable<TResponse> ConfigureAwait(bool continueOnCapturedContext)
        => ResponseAsync.ConfigureAwait(continueOnCapturedContext);

    /// <summary>
    /// Gets the call status if the call has already finished.
    /// Throws InvalidOperationException otherwise.
    /// </summary>
    public Status GetStatus()
        => TryUnwrap().GetStatus();

    /// <summary>
    /// Gets the call trailing metadata if the call has already finished.
    /// Throws InvalidOperationException otherwise.
    /// </summary>
    public Metadata GetTrailers()
        => TryUnwrap().GetTrailers();

    /// <summary>
    /// Provides means to cleanup after the call.
    /// If the call has already finished normally (request stream has been completed and call result has been received), doesn't do anything.
    /// Otherwise, requests cancellation of the call which should terminate all pending async operations associated with the call.
    /// As a result, all resources being used by the call should be released eventually.
    /// </summary>
    /// <remarks>
    /// Normally, there is no need for you to dispose the call unless you want to utilize the
    /// "Cancel" semantics of invoking <c>Dispose</c>.
    /// </remarks>
    public void Dispose()
    {
        if (valueTask is Task<IResponseContext<TResponse>> t)
        {
            if (!t.IsCompleted)
            {
                UnwrapDispose();
            }
            else
            {
                t.Result.Dispose();
            }
        }
    }
}
