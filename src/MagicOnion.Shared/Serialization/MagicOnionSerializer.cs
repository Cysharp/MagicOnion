using MagicOnion.Serialization.MessagePack;

namespace MagicOnion.Serialization
{
    /// <summary>
    /// Provides a serializer for request/response of MagicOnion services and hub methods.
    /// </summary>
    public static class MagicOnionSerializerProvider
    {
        /// <summary>
        /// Gets or sets the <see cref="IMagicOnionSerializerProvider"/> to be used by default.
        /// </summary>
        public static IMagicOnionSerializerProvider Default { get; set; } = MessagePackMagicOnionSerializerProvider.Default;
    }
}
