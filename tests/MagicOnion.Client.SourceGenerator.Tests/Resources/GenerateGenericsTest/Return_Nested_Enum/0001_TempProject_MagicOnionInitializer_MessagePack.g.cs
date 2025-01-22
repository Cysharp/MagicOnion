﻿// <auto-generated />
#pragma warning disable

namespace TempProject
{
    using global::System;
    using global::MessagePack;

    partial class MagicOnionInitializer
    {
        /// <summary>
        /// Gets the generated MessagePack formatter resolver.
        /// </summary>
        public static global::MessagePack.IFormatterResolver Resolver => MessagePackGeneratedResolver.Instance;
        class MessagePackGeneratedResolver : global::MessagePack.IFormatterResolver
        {
            public static readonly global::MessagePack.IFormatterResolver Instance = new MessagePackGeneratedResolver();

            MessagePackGeneratedResolver() {}

            public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
                => FormatterCache<T>.formatter;

            static class FormatterCache<T>
            {
                public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

                static FormatterCache()
                {
                    var f = MessagePackGeneratedGetFormatterHelper.GetFormatter(typeof(T));
                    if (f != null)
                    {
                        formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                    }
                }
            }
        }
        static class MessagePackGeneratedGetFormatterHelper
        {
            static readonly global::System.Collections.Generic.Dictionary<global::System.Type, int> lookup;

            static MessagePackGeneratedGetFormatterHelper()
            {
                lookup = new global::System.Collections.Generic.Dictionary<global::System.Type, int>(3)
                {
                    {typeof(global::TempProject.MyGenericObject<global::TempProject.MyEnum>), 0},
                    {typeof(global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyEnum>>), 1},
                    {typeof(global::TempProject.MyEnum), 2},
                };
            }
            internal static object GetFormatter(global::System.Type t)
            {
                int key;
                if (!lookup.TryGetValue(t, out key))
                {
                    return null;
                }
            
                switch (key)
                {
                    case 0: return new global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyEnum>();
                    case 1: return new global::MessagePack.Formatters.TempProject.MyGenericObjectFormatter<global::TempProject.MyGenericObject<global::TempProject.MyEnum>>();
                    case 2: return new MessagePackEnumFormatters.MyEnumFormatter();
                    default: return null;
                }
            }
        }
        /// <summary>Type hints for Ahead-of-Time compilation.</summary>
        [Preserve]
        static class TypeHints
        {
            [Preserve]
            internal static void Register()
            {
                _ = MessagePackGeneratedResolver.Instance.GetFormatter<global::MessagePack.Nil>();
                _ = MessagePackGeneratedResolver.Instance.GetFormatter<global::TempProject.MyEnum>();
                _ = MessagePackGeneratedResolver.Instance.GetFormatter<global::TempProject.MyGenericObject<global::TempProject.MyEnum>>();
                _ = MessagePackGeneratedResolver.Instance.GetFormatter<global::TempProject.MyGenericObject<global::TempProject.MyGenericObject<global::TempProject.MyEnum>>>();
            }
        }
    }
}
