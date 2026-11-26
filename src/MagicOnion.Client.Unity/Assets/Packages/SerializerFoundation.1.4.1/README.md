SerializerFoundation
===
[![CI](https://github.com/Cysharp/SerializerFoundation/actions/workflows/build-debug.yaml/badge.svg)](https://github.com/Cysharp/SerializerFoundation/actions/workflows/build-debug.yaml)
[![NuGet](https://img.shields.io/nuget/v/SerializerFoundation)](https://www.nuget.org/packages/SerializerFoundation)

High performance serialization infrastructure for C#. The package targets netstandard2.0, netstandard2.1 and net9.0 or greater.

SerializerFoundation is extracted from the [MessagePack-CSharp v4](https://github.com/MessagePack-CSharp/MessagePack-CSharp/tree/v4) rewrite; its [performance that outclasses other serializers](https://github.com/MessagePack-CSharp/MessagePack-CSharp/pull/2294) is built on this foundation. A serializer needs a forward-only buffer, and it needs to support several input and output types such as `Span<byte>`, `IBufferWriter<byte>` and `ReadOnlySequence<byte>`. SerializerFoundation abstracts these behind `IWriteBuffer` and `IReadBuffer`, and provides a buffer layer optimized so that the JIT can specialize code per buffer type.

Beyond serializers, it is also suited to any binary reading and writing, such as implementing a network protocol or laying out a database format.

```bash
dotnet add package SerializerFoundation
```

**Library authors: reference the package with `PrivateAssets="none"`.** If your library depends on SerializerFoundation and is itself published as a NuGet package, NuGet's default `PrivateAssets` strips the bundled analyzers from your package's dependency, so your users never get SF001-SF003. Always declare the reference as:

```xml
<PackageReference Include="SerializerFoundation" Version="1.4.0" PrivateAssets="none" />
```

Serializers write into an `IWriteBuffer` and read from an `IReadBuffer`. Both are deliberately small.

```csharp
public interface IWriteBuffer : IDisposable
{
    long BytesWritten { get; }

    // a writable span of at least sizeHint bytes (sizeHint == 0: some non-empty span)
    Span<byte> GetSpan(int sizeHint = 0);

    // commits bytes written into the span from GetSpan
    void Advance(int bytesWritten);

    // pushes buffered bytes to the underlying destination, when there is one
    void Flush();
}

public interface IReadBuffer : IDisposable
{
    long BytesConsumed { get; }
    long BytesRemaining { get; }

    // the unread part of the current contiguous window, never throws, empty only at end of data
    ReadOnlySpan<byte> GetUnreadSpan();

    // a contiguous window of at least sizeHint bytes, or false if fewer bytes remain
    // copies across segment seams as needed; does not consume bytes
    bool TryGetSpan(int sizeHint, out ReadOnlySpan<byte> span);

    // consumes bytes
    void Advance(int bytesConsumed);

    // copies the next destination.Length bytes without consuming them
    void CopyTo(scoped Span<byte> destination);
}
```

The write side is the familiar `GetSpan` / `Advance` pair from `IBufferWriter<byte>`. The read side has the same shape, reading from a span and then advancing past it, but for optimization it offers several ways to read: `GetUnreadSpan` for whatever is contiguous right now, `TryGetSpan` for a window of a required size across segment seams, and `CopyTo` for copying out without building a contiguous window.

The basic pattern is to build your serializer's definitions on these interfaces, like the following.

```csharp
// basic interface

public interface IFormatter<TWriteBuffer, TReadBuffer, T>
    where TWriteBuffer : struct, IWriteBuffer, allows ref struct
    where TReadBuffer : struct, IReadBuffer, allows ref struct
{
    void Serialize(ref TWriteBuffer buffer, T value);
    T Deserialize(ref TReadBuffer buffer);
}

public readonly record struct Point(int X, int Y);

public sealed class PointFormatter<TWriteBuffer, TReadBuffer> : IFormatter<TWriteBuffer, TReadBuffer, Point>
    where TWriteBuffer : struct, IWriteBuffer, allows ref struct
    where TReadBuffer : struct, IReadBuffer, allows ref struct
{
    public void Serialize(ref TWriteBuffer buffer, Point value)
    {
        buffer.WriteInt32(value.X);
        buffer.WriteInt32(value.Y);
    }

    public Point Deserialize(ref TReadBuffer buffer)
    {
        var x = buffer.ReadInt32();
        var y = buffer.ReadInt32();
        return new Point(x, y);
    }
}
```

```csharp
// Reader/Writer for IReadBuffer/IWriteBuffer

using System.IO;
using System.Buffers.Binary;
using SerializerFoundation;

public static class BufferExtensions
{
    extension<TWriteBuffer>(ref TWriteBuffer buffer)
        where TWriteBuffer : struct, IWriteBuffer, allows ref struct
    {
        public void WriteInt32(int value)
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4), value);
            buffer.Advance(4);
        }
    }

    extension<TReadBuffer>(ref TReadBuffer buffer)
        where TReadBuffer : struct, IReadBuffer, allows ref struct
    {
        public int ReadInt32()
        {
            var span = buffer.GetUnreadSpan();
            if (span.Length < 4 && !buffer.TryGetSpan(4, out span))
            {
                throw new EndOfStreamException();
            }

            var value = BinaryPrimitives.ReadInt32LittleEndian(span);
            buffer.Advance(4);
            return value;
        }
    }
}
```

A serializer writes by requesting the space it needs with `GetSpan` and reporting how much it wrote with `Advance`, and a single serialization may make these calls hundreds or even thousands of times. Keeping the overhead of each call as close to zero as possible is therefore essential. A serializer also has to support both returning a `byte[]` and streaming writes to an `IBufferWriter<byte>`, which is what `PipeWriter` uses. `IWriteBuffer` ships with three implementations, `ArrayPoolListWriteBuffer`, `BufferWriterWriteBuffer` and `SpanWriteBuffer`, each handling its destination efficiently. By defining and calling code in terms of `TWriteBuffer`, `GetSpan` and `Advance` can be devirtualized and inlined regardless of the destination, keeping the overhead minimal.

When deserializing, the buffer has to support `ReadOnlySpan<byte>` as well as `ReadOnlySequence<byte>`, which is what `PipeReader` uses. Both are abstracted as `IReadBuffer`, so they are driven by the same code and can be inlined.

Unlike writing, when deserializing the required buffer size is sometimes unknown until reading begins: integers are commonly variable-length encoded in 1 to 5 bytes, and the length of a string is unknown until its header has been read. In addition, when the source is a `ReadOnlySequence<byte>`, the span you need may not be available as a whole when it straddles a segment boundary. So the flow is to take `GetUnreadSpan` first, call `TryGetSpan` only when it falls short, and finally call `Advance`.

Span lifetime is the one rule to keep in mind. A span from `GetSpan`, `GetUnreadSpan` or `TryGetSpan` stays valid until the next call that hands out a span (or `Flush` / `Dispose`), because that call may hand the window to the destination, return it to a pool, or replace it with a larger one. `Advance` and `CopyTo` do not invalidate a held span. Anything that may touch the buffer, such as a nested formatter, can request a window, so re-request the span after such calls instead of holding on to it.

Write Buffers
---
| Type | Destination | Notes |
| --- | --- | --- |
| `ArrayPoolListWriteBuffer` | caller scratch, then a chain of `ArrayPool<byte>` arrays | The general-purpose `byte[]`-producing buffer. `ToArray()`, `WriteTo(Span<byte>)`, `GetWrittenSegments()`. Dispose returns the rented arrays. |
| `BufferWriterWriteBuffer` | any `IBufferWriter<byte>` (`PipeWriter`, `ArrayBufferWriter<byte>`) | Writes are staged in the current span and committed on a window refill, `Flush` or `Dispose`. |
| `SpanWriteBuffer` | a fixed caller-provided span, such as `stackalloc` memory | Never grows; running out of space throws. For messages with a known maximum size. |

`ArrayPoolListWriteBuffer` supports `byte[] Serialize<T>(T value)` style APIs. It starts with the scratch span you provide, typically `stackalloc` memory. When the next requested window no longer fits, it rents another segment from `ArrayPool<byte>` without copying previously written bytes. Minimum segment sizes grow exponentially, with larger size hints accommodated as needed. `ToArray()` makes the final contiguous copy. This avoids the allocations and copying that a naive implementation incurs by growing through repeated `Array.Resize` calls.

```csharp
Span<byte> scratch = stackalloc byte[512];
var buffer = new ArrayPoolListWriteBuffer(scratch);
try
{
    formatter.Serialize(ref buffer, value);
    return buffer.ToArray();
}
finally
{
    buffer.Dispose();
}
```

When the message has to be post-processed before it reaches its destination, `GetWrittenSegments()` returns a `BufferSegments`: a borrowed, zero-copy view of the written message, valid until the next write or Dispose. Each segment is a non-empty `ReadOnlySpan<byte>`, so a compressor or framing layer that accepts segmented input can consume it directly. Iterate with `TryGetNext`; `Reset()` restarts from the first segment. `SegmentCount` gives the number of segments up front, so a header that depends on it (a block count, a length table) can be written without a counting pass, and `Length` gives the total byte count.

```csharp
var segments = buffer.GetWrittenSegments();
while (segments.TryGetNext(out var segment))
{
    stream.Write(segment);
}
```

`BufferWriterWriteBuffer` connects to the existing `IBufferWriter<byte>` ecosystem, including `PipeWriter` and `ArrayBufferWriter<byte>`. Call `Flush()` when the serializer is done to commit the staged bytes through `IBufferWriter<byte>.Advance`. With a `PipeWriter`, the caller must also call `FlushAsync()` to publish those bytes and observe backpressure.

```csharp
var buffer = new BufferWriterWriteBuffer(pipeWriter);
try
{
    formatter.Serialize(ref buffer, value);
    buffer.Flush();
}
finally
{
    buffer.Dispose();
}
await pipeWriter.FlushAsync();
```

`SerializerFoundation.WriteBufferExtensions.GetReference` is a by-reference shortcut for `GetSpan` with the same contract. For example, an encoder can write a little-endian integer through `Unsafe.WriteUnaligned`:

```csharp
ref byte destination = ref writer.GetReference(sizeof(int));
Unsafe.WriteUnaligned(ref destination, BitConverter.IsLittleEndian ? value : BinaryPrimitives.ReverseEndianness(value));
writer.Advance(sizeof(int));
```

Read Buffers
---
| Type | Source | Notes |
| --- | --- | --- |
| `ReadOnlySpanReadBuffer` | a single contiguous block, `byte[]` or `ReadOnlySpan<byte>` | The standard entry point. `TryGetSpan` never copies. |
| `ReadOnlySequenceReadBuffer` | `ReadOnlySequence<byte>`, such as `PipeReader` output | Reads each contiguous segment in place; bytes that straddle a seam are copied into caller scratch, then into a rented temp only when they do not fit. Dispose returns the temp. |

Wrapping a `byte[]` in `ReadOnlySpanReadBuffer` lets it be consumed as an `IReadBuffer`. The wrapper is very thin, so performance is on par with working directly on a `ReadOnlySpan<byte>` with `Slice`.

Slicing a `ReadOnlySequence<byte>` directly is slow, so processing its internal chain of segments efficiently is critical for performance. `ReadOnlySequenceReadBuffer` does exactly that, and automatically stitches the bytes together when a read straddles a segment boundary. This matters a great deal in binary processing that has to work with spans directly.

```csharp
Span<byte> scratch = stackalloc byte[512];
var buffer = new ReadOnlySequenceReadBuffer(sequence, scratch);
try
{
    var value = formatter.Deserialize(ref buffer);
    long consumed = buffer.BytesConsumed;
    return value;
}
finally
{
    buffer.Dispose();
}
```

`ReadOnlySequenceReadBuffer` can also be constructed over a single `ReadOnlySpan<byte>`, so a reader type that stores it as its only buffer field accepts both kinds of input. The span is served as one window, never stitched or copied. When the input is always contiguous, `ReadOnlySpanReadBuffer` is still the smaller and simpler choice.

`CopyTo(Span<byte>)` copies the next bytes without consuming them, and a multi-segment implementation copies straight out of its segments without building a contiguous window first. It is the right call for fixed-size payloads such as a `byte[]` body or a blittable struct, where a contiguous window would only be an intermediate copy.

Building a Serializer
---
The top-level entry points then choose the buffer for the job and hand it to the formatter. In these method excerpts, `GetFormatter<TWriteBuffer, TReadBuffer, T>()` stands for your serializer's resolver, returning an `IFormatter<TWriteBuffer, TReadBuffer, T>`.

```csharp
public static byte[] Serialize<T>(T value)
{
    Span<byte> scratch = stackalloc byte[256];
    var buffer = new ArrayPoolListWriteBuffer(scratch);
    try
    {
        GetFormatter<ArrayPoolListWriteBuffer, ReadOnlySpanReadBuffer, T>().Serialize(ref buffer, value);
        return buffer.ToArray();
    }
    finally
    {
        buffer.Dispose();
    }
}

public static void Serialize<T>(IBufferWriter<byte> output, T value)
{
    var buffer = new BufferWriterWriteBuffer(output);
    try
    {
        GetFormatter<BufferWriterWriteBuffer, ReadOnlySpanReadBuffer, T>().Serialize(ref buffer, value);
        buffer.Flush();
    }
    finally
    {
        buffer.Dispose();
    }
}

public static T Deserialize<T>(ReadOnlySpan<byte> source)
{
    var buffer = new ReadOnlySpanReadBuffer(source);
    try
    {
        return GetFormatter<ArrayPoolListWriteBuffer, ReadOnlySpanReadBuffer, T>().Deserialize(ref buffer);
    }
    finally
    {
        buffer.Dispose();
    }
}

public static T Deserialize<T>(in ReadOnlySequence<byte> source)
{
    Span<byte> scratch = stackalloc byte[256];
    var buffer = new ReadOnlySequenceReadBuffer(source, scratch);
    try
    {
        return GetFormatter<ArrayPoolListWriteBuffer, ReadOnlySequenceReadBuffer, T>().Deserialize(ref buffer);
    }
    finally
    {
        buffer.Dispose();
    }
}
```

Keep one owner for each buffer. A serializer entry point normally constructs it, passes it by `ref` to formatters, and disposes it in `finally`. A `using` local cannot be passed by `ref` (compiler error CS1657), so these entry points use `try` / `finally`. Caller-provided scratch, input memory and underlying writers are borrowed; disposing a buffer releases its own rented storage or commits its staged writes, without disposing those external resources.

Async and Streaming Interfaces
---
SerializerFoundation provides synchronous interfaces only. Asynchronous operations inside frequently called code degrade performance, and the established approach in such cases is to buffer a reasonable amount of data and then process it synchronously in one go. The core of SerializerFoundation is designed for definitions inside that synchronous processing. For a serializer, memory-conscious asynchronous streaming is only needed when processing a sequence of values, such as an array or JSON Lines. The recommended design is therefore to provide dedicated interfaces for those cases and to `await` on the outside.

Target Frameworks and the Compatible Tier
---
The package ships a net9.0 asset with `allows ref struct` support and netstandard2.0/2.1 assets for older targets. `allows ref struct` requires C# 13 and the .NET 9 runtime, which is exactly where the net9.0 asset starts, so `NET9_0_OR_GREATER` is the condition for selecting between the two tiers.

| Package asset | Buffer tier for generic serializer code |
| --- | --- |
| net9.0 and later | `ArrayPoolListWriteBuffer`, `BufferWriterWriteBuffer`, `SpanWriteBuffer`, `ReadOnlySpanReadBuffer`, `ReadOnlySequenceReadBuffer` (all `ref struct`) and the `Compatible*` variants |
| netstandard2.0, netstandard2.1 | `CompatibleArrayPoolListWriteBuffer`, `CompatibleBufferWriterWriteBuffer`, `CompatibleSpanWriteBuffer`, `CompatibleReadOnlySpanReadBuffer`, `CompatibleReadOnlySequenceReadBuffer` (plain `struct`) |

The `ref struct` buffers exist on every target and can be used directly. Generic serializer code targeting netstandard uses the `Compatible*` tier, because a `ref struct` cannot be a type argument without `allows ref struct`.

The `Compatible*` structs implement the same interfaces with the same semantics. `CompatibleArrayPoolListWriteBuffer` starts with pooled storage and takes no scratch span; `CompatibleReadOnlySequenceReadBuffer` uses a rented temporary array when stitching is necessary. The span-based variants (`CompatibleSpanWriteBuffer`, `CompatibleReadOnlySpanReadBuffer`) take a `byte*` and length instead of a span. Keep that memory valid for the buffer's lifetime, and pin it when it belongs to a managed object.

The netstandard assets cover compatible .NET Framework and engine runtimes such as Unity and Godot. Runtime compatibility and compiler support are separate: older compilers can use the `Compatible*` structs, while defining the extension-block syntax in this README requires C# 14. The equivalent traditional syntax is `public static void WriteInt32<TBuffer>(this ref TBuffer buffer, int value) where TBuffer : struct, IWriteBuffer`.

A formatter that multi-targets writes its constraints once with a conditional `allows ref struct`.

```csharp
public sealed class PointFormatter<TWriteBuffer, TReadBuffer> : IFormatter<TWriteBuffer, TReadBuffer, Point>
    where TWriteBuffer : struct, IWriteBuffer
#if NET9_0_OR_GREATER
    , allows ref struct
#endif
    where TReadBuffer : struct, IReadBuffer
#if NET9_0_OR_GREATER
    , allows ref struct
#endif
{
    // the body is identical on every target
}
```

Use the same conditional constraints on `IFormatter` and on generic helper methods. The serializer's entry points then pick the tier per target. A serializer that loads formatter assemblies built for different targets also needs a resolver strategy for selecting a compatible formatter; that policy belongs to the serializer.

```csharp
public static byte[] Serialize<T>(T value)
{
#if NET9_0_OR_GREATER
    Span<byte> scratch = stackalloc byte[256];
    var buffer = new ArrayPoolListWriteBuffer(scratch);
    try
    {
        GetFormatter<ArrayPoolListWriteBuffer, ReadOnlySpanReadBuffer, T>().Serialize(ref buffer, value);
        return buffer.ToArray();
    }
    finally
    {
        buffer.Dispose();
    }
#else
    var buffer = new CompatibleArrayPoolListWriteBuffer();
    try
    {
        GetFormatter<CompatibleArrayPoolListWriteBuffer, CompatibleReadOnlySpanReadBuffer, T>().Serialize(ref buffer, value);
        return buffer.ToArray();
    }
    finally
    {
        buffer.Dispose();
    }
#endif
}

public static unsafe T Deserialize<T>(ReadOnlySpan<byte> source)
{
#if NET9_0_OR_GREATER
    var buffer = new ReadOnlySpanReadBuffer(source);
    try
    {
        return GetFormatter<ArrayPoolListWriteBuffer, ReadOnlySpanReadBuffer, T>().Deserialize(ref buffer);
    }
    finally
    {
        buffer.Dispose();
    }
#else
    fixed (byte* pointer = source)
    {
        var buffer = new CompatibleReadOnlySpanReadBuffer(pointer, source.Length);
        try
        {
            return GetFormatter<CompatibleArrayPoolListWriteBuffer, CompatibleReadOnlySpanReadBuffer, T>().Deserialize(ref buffer);
        }
        finally
        {
            buffer.Dispose();
        }
    }
#endif
}
```

`BufferSegments` is the same type on every target, so message processors (compressors, encryptors, framers) that consume it multi-target without any `#if`.

Custom IWriteBuffer / IReadBuffer Implementations
---
Beyond the built-in `IWriteBuffer` / `IReadBuffer` implementations, you can provide your own specialized buffer by implementing the interfaces yourself. Follow the contract of each member below. Implementations must be structs (`ref struct` allowed) and are single-owner: they are passed by `ref` and never copied. `Dispose` releases only what the buffer itself rented or staged, never caller-provided scratch, input memory or the underlying writer.

| `IWriteBuffer` | Contract |
| --- | --- |
| `BytesWritten` | Total bytes committed through `Advance`. |
| `GetSpan(sizeHint)` | A writable span of at least `sizeHint` bytes; `0` returns some non-empty span; negative throws `ArgumentOutOfRangeException`. A size the buffer cannot provide throws `InvalidOperationException` when it is out of room, or lets the failed allocation surface when it grows by allocating (`OutOfMemoryException` past the largest possible array). Valid until the next `GetSpan`, `Flush` or `Dispose`. |
| `Advance(bytesWritten)` | Commits the leading bytes of the last span; negative or beyond the span throws `InvalidOperationException`. The rest of the span stays writable. |
| `Flush()` | Pushes buffered bytes to the destination when there is one; otherwise a no-op. May invalidate a held span. |
| `Dispose()` | Commits or releases the buffer's own state. Called once by the owner. |

| `IReadBuffer` | Contract |
| --- | --- |
| `BytesConsumed` | Total bytes consumed through `Advance`. |
| `BytesRemaining` | Unread bytes left in the data. |
| `GetUnreadSpan()` | The unread part of the current contiguous window, any size. Empty only when the data is exhausted; a segmented source repositions onto the next non-empty segment. Never throws, never consumes. Valid until the next `GetUnreadSpan`, `TryGetSpan` or `Dispose`. |
| `TryGetSpan(sizeHint, out span)` | A contiguous window of at least `sizeHint` bytes, copying across seams as needed, or `false` when fewer remain; `0` always succeeds; negative throws `ArgumentOutOfRangeException`. Never consumes. Same lifetime as `GetUnreadSpan`; a stitched window lives in temporary storage the next request reuses. |
| `Advance(bytesConsumed)` | Consumes bytes; negative or beyond `BytesRemaining` throws `InvalidOperationException`. Does not invalidate a held span. |
| `CopyTo(destination)` | Copies the next `destination.Length` bytes without consuming them; longer than `BytesRemaining` throws `InvalidOperationException`. Multi-segment sources copy straight from their segments. The parameter is `scoped` (never retained, so callers may pass stack memory); a `ref struct` implementation must declare it `scoped` too. |
| `Dispose()` | Releases temporary storage the buffer rented. Called once by the owner. |

RequireOverride
---
When a multi-targeted base class adds a generic buffer method only on its net9.0 target, making that method `abstract` would break subclasses compiled against its netstandard target: they have no implementation of the new abstract method. A `virtual` method with a bridge body keeps those subclasses usable, while newly compiled subclasses can override it with a direct implementation.

`[RequireOverride]` marks such a virtual method as conceptually abstract. The bundled SF003 analyzer requires every non-abstract derived type that can access the method to override it, directly or through a base class. Downlevel compilations never see the method, so they have no override requirement. The following `MessageProcessor` is an example base class for your serializer.

```csharp
using System.Buffers;
using SerializerFoundation;
using SerializerFoundation.CodeAnalysis;

public abstract class MessageProcessor
{
    // every target: the interface-shaped entry
    public abstract bool TryEncode(ref BufferSegments message, IBufferWriter<byte> output);

#if NET9_0_OR_GREATER
    // net9.0 and later: writes straight into the target buffer.
    // virtual so netstandard subclasses keep loading; [RequireOverride] so modern subclasses cannot forget it.
    [RequireOverride]
    public virtual bool TryEncode<TWriteBuffer>(ref BufferSegments message, ref TWriteBuffer output)
        where TWriteBuffer : struct, IWriteBuffer, allows ref struct
    {
        // bridge for netstandard subclasses: encode through the interface overload, then copy (one extra copy)
        var staging = new ArrayBufferWriter<byte>();
        if (!TryEncode(ref message, staging)) return false;
        if (staging.WrittenCount > 0)
        {
            staging.WrittenSpan.CopyTo(output.GetSpan(staging.WrittenCount));
            output.Advance(staging.WrittenCount);
        }
        return true;
    }
#endif
}
```

The attribute is not inherited. An override satisfies the requirement for everything below it, and an intermediate override that should itself be overridden again applies the attribute anew.

Analyzers
---
The package bundles Roslyn analyzers that check the buffer contract at compile time. All three diagnostics have error severity by default: SF001 catches buffer implementations that cannot satisfy a serializer's struct constraint, SF002 catches common ownership mistakes, and SF003 enforces explicitly required overrides.

| ID | Description |
| --- | --- |
| SF001 | `IWriteBuffer` / `IReadBuffer` implementations must be structs. Every consuming API constrains buffers to `struct`, so a class implementation compiles at its declaration and is unusable at every call site. |
| SF002 | Buffer structs are single-owner and must not be copied. A copy diverges silently (two write indexes over one window) and double-returns pooled arrays at Dispose. |
| SF003 | Virtual methods marked `[RequireOverride]` must be overridden by concrete derived types that can access them. |

SF002 is the one you will meet. Implementing a buffer interface is the non-copyable marker, so buffers need no attribute. A struct that is not a buffer but holds one by value, such as a serializer's reader or writer, opts in with `[NonCopyable]` (`SerializerFoundation.CodeAnalysis`) and is checked by the same rules. The analyzer catches common copy mistakes in serializer code: by-value parameters and extension receivers, `in` / `ref readonly` parameters (the callee mutates a defensive copy), local and field initialization from another buffer, boxing, capture in a tuple or array initializer, `foreach`, pattern matching, `with`, by-value properties, yielding an existing buffer, and mutation through a `readonly` reference. It is a guard against common mistakes, not a complete ownership checker: callers still need to keep buffers single-owner and pass them by `ref`.

```csharp
void ByValue(ArrayPoolListWriteBuffer buffer) { }    // SF002
void ByReadonlyRef(in ArrayPoolListWriteBuffer buffer) { } // SF002
void ByRef(ref ArrayPoolListWriteBuffer buffer) { }  // OK

var buffer = new ArrayPoolListWriteBuffer(scratch); // OK: constructed in place
var copy = buffer;                                 // SF002: copied by assignment

[NonCopyable]
public ref struct MyWriter<TBuffer> where TBuffer : struct, IWriteBuffer, allows ref struct
{
    TBuffer buffer; // a copy of MyWriter would copy the buffer too, so MyWriter follows the same rules
}
```

The analyzers ship inside the SerializerFoundation package under `analyzers/dotnet/cs` and require a host compatible with Roslyn 4.3.1 or later. A direct `PackageReference` loads them automatically. Transitive references are different: NuGet's default `PrivateAssets` (`contentfiles;analyzers;build`) writes `exclude="Build,Analyzers"` into the dependency entry of any package that references SerializerFoundation, so applications that only reference that package never load the analyzers. **If you publish a library that depends on SerializerFoundation, always reference it with `PrivateAssets="none"`** so the analyzers reach your users:

```xml
<PackageReference Include="SerializerFoundation" Version="1.4.0" PrivateAssets="none" />
```

When importing DLLs manually or through an engine-specific workflow, configure that host to load the analyzer DLL as well.

License
---
This library is under the MIT License.
