#region Copyright notice and license

// Copyright 2015, Google Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//     * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
//     * Neither the name of Google Inc. nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using Grpc.Core.Logging;
using Grpc.Core.Utils;

namespace Grpc.Core.Internal
{
    /// <summary>
    /// Provides access to all native methods provided by <c>NativeExtension</c>.
    /// An extra level of indirection is added to P/Invoke calls to allow intelligent loading
    /// of the right configuration of the native extension based on current platform, architecture etc.
    /// </summary>
    internal class NativeMethods
    {
#if UNITY_EDITOR		
        private const string pluginName = "grpc_csharp_ext";		
#elif UNITY_IOS || UNITY_TVOS || UNITY_WEBGL		
        public const string pluginName = "__Internal";		
#else		
        public const string pluginName = "grpc_csharp_ext";		
#endif

        #region Native methods

        public readonly Delegates.grpcsharp_init_delegate grpcsharp_init;
        public readonly Delegates.grpcsharp_shutdown_delegate grpcsharp_shutdown;
        public readonly Delegates.grpcsharp_version_string_delegate grpcsharp_version_string;

        public readonly Delegates.grpcsharp_batch_context_create_delegate grpcsharp_batch_context_create;
        public readonly Delegates.grpcsharp_batch_context_recv_initial_metadata_delegate grpcsharp_batch_context_recv_initial_metadata;
        public readonly Delegates.grpcsharp_batch_context_recv_message_length_delegate grpcsharp_batch_context_recv_message_length;
        public readonly Delegates.grpcsharp_batch_context_recv_message_to_buffer_delegate grpcsharp_batch_context_recv_message_to_buffer;
        public readonly Delegates.grpcsharp_batch_context_recv_status_on_client_status_delegate grpcsharp_batch_context_recv_status_on_client_status;
        public readonly Delegates.grpcsharp_batch_context_recv_status_on_client_details_delegate grpcsharp_batch_context_recv_status_on_client_details;
        public readonly Delegates.grpcsharp_batch_context_recv_status_on_client_trailing_metadata_delegate grpcsharp_batch_context_recv_status_on_client_trailing_metadata;
        public readonly Delegates.grpcsharp_batch_context_recv_close_on_server_cancelled_delegate grpcsharp_batch_context_recv_close_on_server_cancelled;
        public readonly Delegates.grpcsharp_batch_context_destroy_delegate grpcsharp_batch_context_destroy;

        public readonly Delegates.grpcsharp_request_call_context_destroy_delegate grpcsharp_request_call_context_destroy;

        public readonly Delegates.grpcsharp_composite_call_credentials_create_delegate grpcsharp_composite_call_credentials_create;
        public readonly Delegates.grpcsharp_call_credentials_release_delegate grpcsharp_call_credentials_release;

        public readonly Delegates.grpcsharp_call_cancel_delegate grpcsharp_call_cancel;
        public readonly Delegates.grpcsharp_call_cancel_with_status_delegate grpcsharp_call_cancel_with_status;
        public readonly Delegates.grpcsharp_call_start_unary_delegate grpcsharp_call_start_unary;
        public readonly Delegates.grpcsharp_call_start_client_streaming_delegate grpcsharp_call_start_client_streaming;
        public readonly Delegates.grpcsharp_call_start_server_streaming_delegate grpcsharp_call_start_server_streaming;
        public readonly Delegates.grpcsharp_call_start_duplex_streaming_delegate grpcsharp_call_start_duplex_streaming;
        public readonly Delegates.grpcsharp_call_send_message_delegate grpcsharp_call_send_message;
        public readonly Delegates.grpcsharp_call_send_close_from_client_delegate grpcsharp_call_send_close_from_client;
        public readonly Delegates.grpcsharp_call_send_status_from_server_delegate grpcsharp_call_send_status_from_server;
        public readonly Delegates.grpcsharp_call_recv_message_delegate grpcsharp_call_recv_message;
        public readonly Delegates.grpcsharp_call_recv_initial_metadata_delegate grpcsharp_call_recv_initial_metadata;
        public readonly Delegates.grpcsharp_call_start_serverside_delegate grpcsharp_call_start_serverside;
        public readonly Delegates.grpcsharp_call_send_initial_metadata_delegate grpcsharp_call_send_initial_metadata;
        public readonly Delegates.grpcsharp_call_set_credentials_delegate grpcsharp_call_set_credentials;
        public readonly Delegates.grpcsharp_call_get_peer_delegate grpcsharp_call_get_peer;
        public readonly Delegates.grpcsharp_call_destroy_delegate grpcsharp_call_destroy;

        public readonly Delegates.grpcsharp_channel_args_create_delegate grpcsharp_channel_args_create;
        public readonly Delegates.grpcsharp_channel_args_set_string_delegate grpcsharp_channel_args_set_string;
        public readonly Delegates.grpcsharp_channel_args_set_integer_delegate grpcsharp_channel_args_set_integer;
        public readonly Delegates.grpcsharp_channel_args_destroy_delegate grpcsharp_channel_args_destroy;

        public readonly Delegates.grpcsharp_override_default_ssl_roots grpcsharp_override_default_ssl_roots;
        public readonly Delegates.grpcsharp_ssl_credentials_create_delegate grpcsharp_ssl_credentials_create;
        public readonly Delegates.grpcsharp_composite_channel_credentials_create_delegate grpcsharp_composite_channel_credentials_create;
        public readonly Delegates.grpcsharp_channel_credentials_release_delegate grpcsharp_channel_credentials_release;

        public readonly Delegates.grpcsharp_insecure_channel_create_delegate grpcsharp_insecure_channel_create;
        public readonly Delegates.grpcsharp_secure_channel_create_delegate grpcsharp_secure_channel_create;
        public readonly Delegates.grpcsharp_channel_create_call_delegate grpcsharp_channel_create_call;
        public readonly Delegates.grpcsharp_channel_check_connectivity_state_delegate grpcsharp_channel_check_connectivity_state;
        public readonly Delegates.grpcsharp_channel_watch_connectivity_state_delegate grpcsharp_channel_watch_connectivity_state;
        public readonly Delegates.grpcsharp_channel_get_target_delegate grpcsharp_channel_get_target;
        public readonly Delegates.grpcsharp_channel_destroy_delegate grpcsharp_channel_destroy;

        public readonly Delegates.grpcsharp_sizeof_grpc_event_delegate grpcsharp_sizeof_grpc_event;

        public readonly Delegates.grpcsharp_completion_queue_create_delegate grpcsharp_completion_queue_create;
        public readonly Delegates.grpcsharp_completion_queue_shutdown_delegate grpcsharp_completion_queue_shutdown;
        public readonly Delegates.grpcsharp_completion_queue_next_delegate grpcsharp_completion_queue_next;
#if UNITY_EDITOR
        public readonly Delegates.grpcsharp_completion_queue_next_debuggable_delegate grpcsharp_completion_queue_next_debuggable;
#endif
        public readonly Delegates.grpcsharp_completion_queue_pluck_delegate grpcsharp_completion_queue_pluck;
        public readonly Delegates.grpcsharp_completion_queue_destroy_delegate grpcsharp_completion_queue_destroy;

        public readonly Delegates.gprsharp_free_delegate gprsharp_free;

        public readonly Delegates.grpcsharp_metadata_array_create_delegate grpcsharp_metadata_array_create;
        public readonly Delegates.grpcsharp_metadata_array_add_delegate grpcsharp_metadata_array_add;
        public readonly Delegates.grpcsharp_metadata_array_count_delegate grpcsharp_metadata_array_count;
        public readonly Delegates.grpcsharp_metadata_array_get_key_delegate grpcsharp_metadata_array_get_key;
        public readonly Delegates.grpcsharp_metadata_array_get_value_delegate grpcsharp_metadata_array_get_value;
        public readonly Delegates.grpcsharp_metadata_array_destroy_full_delegate grpcsharp_metadata_array_destroy_full;

        public readonly Delegates.grpcsharp_redirect_log_delegate grpcsharp_redirect_log;

        public readonly Delegates.grpcsharp_metadata_credentials_create_from_plugin_delegate grpcsharp_metadata_credentials_create_from_plugin;
        public readonly Delegates.grpcsharp_metadata_credentials_notify_from_plugin_delegate grpcsharp_metadata_credentials_notify_from_plugin;

        public readonly Delegates.grpcsharp_server_credentials_release_delegate grpcsharp_server_credentials_release;

        public readonly Delegates.grpcsharp_server_destroy_delegate grpcsharp_server_destroy;

        public readonly Delegates.grpcsharp_call_auth_context_delegate grpcsharp_call_auth_context;
        public readonly Delegates.grpcsharp_auth_context_peer_identity_property_name_delegate grpcsharp_auth_context_peer_identity_property_name;
        public readonly Delegates.grpcsharp_auth_context_property_iterator_delegate grpcsharp_auth_context_property_iterator;
        public readonly Delegates.grpcsharp_auth_property_iterator_next_delegate grpcsharp_auth_property_iterator_next;
        public readonly Delegates.grpcsharp_auth_context_release_delegate grpcsharp_auth_context_release;

        public readonly Delegates.gprsharp_now_delegate gprsharp_now;
        public readonly Delegates.gprsharp_inf_future_delegate gprsharp_inf_future;
        public readonly Delegates.gprsharp_inf_past_delegate gprsharp_inf_past;
        public readonly Delegates.gprsharp_convert_clock_type_delegate gprsharp_convert_clock_type;
        public readonly Delegates.gprsharp_sizeof_timespec_delegate gprsharp_sizeof_timespec;

        public readonly Delegates.grpcsharp_test_callback_delegate grpcsharp_test_callback;
        public readonly Delegates.grpcsharp_test_nop_delegate grpcsharp_test_nop;

        #endregion

        public NativeMethods()
        {
            this.grpcsharp_init = NativeCalls.grpcsharp_init;
            this.grpcsharp_shutdown = NativeCalls.grpcsharp_shutdown;
            this.grpcsharp_version_string = NativeCalls.grpcsharp_version_string;

            this.grpcsharp_batch_context_create = NativeCalls.grpcsharp_batch_context_create;
            this.grpcsharp_batch_context_recv_initial_metadata = NativeCalls.grpcsharp_batch_context_recv_initial_metadata;
            this.grpcsharp_batch_context_recv_message_length = NativeCalls.grpcsharp_batch_context_recv_message_length;
            this.grpcsharp_batch_context_recv_message_to_buffer = NativeCalls.grpcsharp_batch_context_recv_message_to_buffer;
            this.grpcsharp_batch_context_recv_status_on_client_status = NativeCalls.grpcsharp_batch_context_recv_status_on_client_status;
            this.grpcsharp_batch_context_recv_status_on_client_details = NativeCalls.grpcsharp_batch_context_recv_status_on_client_details;
            this.grpcsharp_batch_context_recv_status_on_client_trailing_metadata = NativeCalls.grpcsharp_batch_context_recv_status_on_client_trailing_metadata;
            this.grpcsharp_batch_context_recv_close_on_server_cancelled = NativeCalls.grpcsharp_batch_context_recv_close_on_server_cancelled;
            this.grpcsharp_batch_context_destroy = NativeCalls.grpcsharp_batch_context_destroy;

            this.grpcsharp_request_call_context_destroy = NativeCalls.grpcsharp_request_call_context_destroy;

            this.grpcsharp_composite_call_credentials_create = NativeCalls.grpcsharp_composite_call_credentials_create;
            this.grpcsharp_call_credentials_release = NativeCalls.grpcsharp_call_credentials_release;

            this.grpcsharp_call_cancel = NativeCalls.grpcsharp_call_cancel;
            this.grpcsharp_call_cancel_with_status = NativeCalls.grpcsharp_call_cancel_with_status;
            this.grpcsharp_call_start_unary = NativeCalls.grpcsharp_call_start_unary;
            this.grpcsharp_call_start_client_streaming = NativeCalls.grpcsharp_call_start_client_streaming;
            this.grpcsharp_call_start_server_streaming = NativeCalls.grpcsharp_call_start_server_streaming;
            this.grpcsharp_call_start_duplex_streaming = NativeCalls.grpcsharp_call_start_duplex_streaming;
            this.grpcsharp_call_send_message = NativeCalls.grpcsharp_call_send_message;
            this.grpcsharp_call_send_close_from_client = NativeCalls.grpcsharp_call_send_close_from_client;
            this.grpcsharp_call_send_status_from_server = NativeCalls.grpcsharp_call_send_status_from_server;
            this.grpcsharp_call_recv_message = NativeCalls.grpcsharp_call_recv_message;
            this.grpcsharp_call_recv_initial_metadata = NativeCalls.grpcsharp_call_recv_initial_metadata;
            this.grpcsharp_call_start_serverside = NativeCalls.grpcsharp_call_start_serverside;
            this.grpcsharp_call_send_initial_metadata = NativeCalls.grpcsharp_call_send_initial_metadata;
            this.grpcsharp_call_set_credentials = NativeCalls.grpcsharp_call_set_credentials;
            this.grpcsharp_call_get_peer = NativeCalls.grpcsharp_call_get_peer;
            this.grpcsharp_call_destroy = NativeCalls.grpcsharp_call_destroy;

            this.grpcsharp_channel_args_create = NativeCalls.grpcsharp_channel_args_create;
            this.grpcsharp_channel_args_set_string = NativeCalls.grpcsharp_channel_args_set_string;
            this.grpcsharp_channel_args_set_integer = NativeCalls.grpcsharp_channel_args_set_integer;
            this.grpcsharp_channel_args_destroy = NativeCalls.grpcsharp_channel_args_destroy;

            this.grpcsharp_override_default_ssl_roots = NativeCalls.grpcsharp_override_default_ssl_roots;
            this.grpcsharp_ssl_credentials_create = NativeCalls.grpcsharp_ssl_credentials_create;
            this.grpcsharp_composite_channel_credentials_create = NativeCalls.grpcsharp_composite_channel_credentials_create;
            this.grpcsharp_channel_credentials_release = NativeCalls.grpcsharp_channel_credentials_release;

            this.grpcsharp_insecure_channel_create = NativeCalls.grpcsharp_insecure_channel_create;
            this.grpcsharp_secure_channel_create = NativeCalls.grpcsharp_secure_channel_create;
            this.grpcsharp_channel_create_call = NativeCalls.grpcsharp_channel_create_call;
            this.grpcsharp_channel_check_connectivity_state = NativeCalls.grpcsharp_channel_check_connectivity_state;
            this.grpcsharp_channel_watch_connectivity_state = NativeCalls.grpcsharp_channel_watch_connectivity_state;
            this.grpcsharp_channel_get_target = NativeCalls.grpcsharp_channel_get_target;
            this.grpcsharp_channel_destroy = NativeCalls.grpcsharp_channel_destroy;

            this.grpcsharp_sizeof_grpc_event = NativeCalls.grpcsharp_sizeof_grpc_event;

            this.grpcsharp_completion_queue_create = NativeCalls.grpcsharp_completion_queue_create;
            this.grpcsharp_completion_queue_shutdown = NativeCalls.grpcsharp_completion_queue_shutdown;
            this.grpcsharp_completion_queue_next = NativeCalls.grpcsharp_completion_queue_next;
#if UNITY_EDITOR
            this.grpcsharp_completion_queue_next_debuggable = NativeCalls.grpcsharp_completion_queue_next_debuggable;
#endif
            this.grpcsharp_completion_queue_pluck = NativeCalls.grpcsharp_completion_queue_pluck;
            this.grpcsharp_completion_queue_destroy = NativeCalls.grpcsharp_completion_queue_destroy;

            this.gprsharp_free = NativeCalls.gprsharp_free;

            this.grpcsharp_metadata_array_create = NativeCalls.grpcsharp_metadata_array_create;
            this.grpcsharp_metadata_array_add = NativeCalls.grpcsharp_metadata_array_add;
            this.grpcsharp_metadata_array_count = NativeCalls.grpcsharp_metadata_array_count;
            this.grpcsharp_metadata_array_get_key = NativeCalls.grpcsharp_metadata_array_get_key;
            this.grpcsharp_metadata_array_get_value = NativeCalls.grpcsharp_metadata_array_get_value;
            this.grpcsharp_metadata_array_destroy_full = NativeCalls.grpcsharp_metadata_array_destroy_full;

            this.grpcsharp_redirect_log = NativeCalls.grpcsharp_redirect_log;

            this.grpcsharp_metadata_credentials_create_from_plugin = NativeCalls.grpcsharp_metadata_credentials_create_from_plugin;
            this.grpcsharp_metadata_credentials_notify_from_plugin = NativeCalls.grpcsharp_metadata_credentials_notify_from_plugin;

            this.grpcsharp_server_credentials_release = NativeCalls.grpcsharp_server_credentials_release;

            this.grpcsharp_server_destroy = NativeCalls.grpcsharp_server_destroy;

            this.grpcsharp_call_auth_context = NativeCalls.grpcsharp_call_auth_context;
            this.grpcsharp_auth_context_peer_identity_property_name = NativeCalls.grpcsharp_auth_context_peer_identity_property_name;
            this.grpcsharp_auth_context_property_iterator = NativeCalls.grpcsharp_auth_context_property_iterator;
            this.grpcsharp_auth_property_iterator_next = NativeCalls.grpcsharp_auth_property_iterator_next;
            this.grpcsharp_auth_context_release = NativeCalls.grpcsharp_auth_context_release;

            this.gprsharp_now = NativeCalls.gprsharp_now;
            this.gprsharp_inf_future = NativeCalls.gprsharp_inf_future;
            this.gprsharp_inf_past = NativeCalls.gprsharp_inf_past;
            this.gprsharp_convert_clock_type = NativeCalls.gprsharp_convert_clock_type;
            this.gprsharp_sizeof_timespec = NativeCalls.gprsharp_sizeof_timespec;

            this.grpcsharp_test_callback = NativeCalls.grpcsharp_test_callback;
            this.grpcsharp_test_nop = NativeCalls.grpcsharp_test_nop;
        }

        /// <summary>
        /// Gets singleton instance of this class.
        /// </summary>
        public static NativeMethods Get()
        {
            return NativeExtension.Get().NativeMethods;
        }

        /// <summary>
        /// Delegate types for all published native methods. Declared under inner class to prevent scope pollution.
        /// </summary>
        public class Delegates
        {
            public delegate void grpcsharp_init_delegate();
            public delegate void grpcsharp_shutdown_delegate();
            public delegate IntPtr grpcsharp_version_string_delegate();  // returns not-owned const char*

            public delegate BatchContextSafeHandle grpcsharp_batch_context_create_delegate();
            public delegate IntPtr grpcsharp_batch_context_recv_initial_metadata_delegate(BatchContextSafeHandle ctx);
            public delegate IntPtr grpcsharp_batch_context_recv_message_length_delegate(BatchContextSafeHandle ctx);
            public delegate void grpcsharp_batch_context_recv_message_to_buffer_delegate(BatchContextSafeHandle ctx, byte[] buffer, UIntPtr bufferLen);
            public delegate StatusCode grpcsharp_batch_context_recv_status_on_client_status_delegate(BatchContextSafeHandle ctx);
            public delegate IntPtr grpcsharp_batch_context_recv_status_on_client_details_delegate(BatchContextSafeHandle ctx, out UIntPtr detailsLength);
            public delegate IntPtr grpcsharp_batch_context_recv_status_on_client_trailing_metadata_delegate(BatchContextSafeHandle ctx);
            public delegate int grpcsharp_batch_context_recv_close_on_server_cancelled_delegate(BatchContextSafeHandle ctx);
            public delegate void grpcsharp_batch_context_destroy_delegate(IntPtr ctx);

            public delegate void grpcsharp_request_call_context_destroy_delegate(IntPtr ctx);

            public delegate CallCredentialsSafeHandle grpcsharp_composite_call_credentials_create_delegate(CallCredentialsSafeHandle creds1, CallCredentialsSafeHandle creds2);
            public delegate void grpcsharp_call_credentials_release_delegate(IntPtr credentials);

            public delegate CallError grpcsharp_call_cancel_delegate(CallSafeHandle call);
            public delegate CallError grpcsharp_call_cancel_with_status_delegate(CallSafeHandle call, StatusCode status, string description);
            public delegate CallError grpcsharp_call_start_unary_delegate(CallSafeHandle call,
                BatchContextSafeHandle ctx, byte[] sendBuffer, UIntPtr sendBufferLen, WriteFlags writeFlags, MetadataArraySafeHandle metadataArray, CallFlags metadataFlags);
            public delegate CallError grpcsharp_call_start_client_streaming_delegate(CallSafeHandle call,
                BatchContextSafeHandle ctx, MetadataArraySafeHandle metadataArray, CallFlags metadataFlags);
            public delegate CallError grpcsharp_call_start_server_streaming_delegate(CallSafeHandle call,
                BatchContextSafeHandle ctx, byte[] sendBuffer, UIntPtr sendBufferLen, WriteFlags writeFlags,
                MetadataArraySafeHandle metadataArray, CallFlags metadataFlags);
            public delegate CallError grpcsharp_call_start_duplex_streaming_delegate(CallSafeHandle call,
                BatchContextSafeHandle ctx, MetadataArraySafeHandle metadataArray, CallFlags metadataFlags);
            public delegate CallError grpcsharp_call_send_message_delegate(CallSafeHandle call,
                BatchContextSafeHandle ctx, byte[] sendBuffer, UIntPtr sendBufferLen, WriteFlags writeFlags, bool sendEmptyInitialMetadata);
            public delegate CallError grpcsharp_call_send_close_from_client_delegate(CallSafeHandle call,
                BatchContextSafeHandle ctx);
            public delegate CallError grpcsharp_call_send_status_from_server_delegate(CallSafeHandle call,
                BatchContextSafeHandle ctx, StatusCode statusCode, byte[] statusMessage, UIntPtr statusMessageLen, MetadataArraySafeHandle metadataArray, bool sendEmptyInitialMetadata,
                byte[] optionalSendBuffer, UIntPtr optionalSendBufferLen, WriteFlags writeFlags);
            public delegate CallError grpcsharp_call_recv_message_delegate(CallSafeHandle call,
                BatchContextSafeHandle ctx);
            public delegate CallError grpcsharp_call_recv_initial_metadata_delegate(CallSafeHandle call,
                BatchContextSafeHandle ctx);
            public delegate CallError grpcsharp_call_start_serverside_delegate(CallSafeHandle call,
                BatchContextSafeHandle ctx);
            public delegate CallError grpcsharp_call_send_initial_metadata_delegate(CallSafeHandle call,
                BatchContextSafeHandle ctx, MetadataArraySafeHandle metadataArray);
            public delegate CallError grpcsharp_call_set_credentials_delegate(CallSafeHandle call, CallCredentialsSafeHandle credentials);
            public delegate CStringSafeHandle grpcsharp_call_get_peer_delegate(CallSafeHandle call);
            public delegate void grpcsharp_call_destroy_delegate(IntPtr call);

            public delegate ChannelArgsSafeHandle grpcsharp_channel_args_create_delegate(UIntPtr numArgs);
            public delegate void grpcsharp_channel_args_set_string_delegate(ChannelArgsSafeHandle args, UIntPtr index, string key, string value);
            public delegate void grpcsharp_channel_args_set_integer_delegate(ChannelArgsSafeHandle args, UIntPtr index, string key, int value);
            public delegate void grpcsharp_channel_args_destroy_delegate(IntPtr args);

            public delegate void grpcsharp_override_default_ssl_roots(string pemRootCerts);
            public delegate ChannelCredentialsSafeHandle grpcsharp_ssl_credentials_create_delegate(string pemRootCerts, string keyCertPairCertChain, string keyCertPairPrivateKey);
            public delegate ChannelCredentialsSafeHandle grpcsharp_composite_channel_credentials_create_delegate(ChannelCredentialsSafeHandle channelCreds, CallCredentialsSafeHandle callCreds);
            public delegate void grpcsharp_channel_credentials_release_delegate(IntPtr credentials);

            public delegate ChannelSafeHandle grpcsharp_insecure_channel_create_delegate(string target, ChannelArgsSafeHandle channelArgs);
            public delegate ChannelSafeHandle grpcsharp_secure_channel_create_delegate(ChannelCredentialsSafeHandle credentials, string target, ChannelArgsSafeHandle channelArgs);
            public delegate CallSafeHandle grpcsharp_channel_create_call_delegate(ChannelSafeHandle channel, CallSafeHandle parentCall, ContextPropagationFlags propagationMask, CompletionQueueSafeHandle cq, string method, string host, Timespec deadline);
            public delegate ChannelState grpcsharp_channel_check_connectivity_state_delegate(ChannelSafeHandle channel, int tryToConnect);
            public delegate void grpcsharp_channel_watch_connectivity_state_delegate(ChannelSafeHandle channel, ChannelState lastObservedState,
                Timespec deadline, CompletionQueueSafeHandle cq, BatchContextSafeHandle ctx);
            public delegate CStringSafeHandle grpcsharp_channel_get_target_delegate(ChannelSafeHandle call);
            public delegate void grpcsharp_channel_destroy_delegate(IntPtr channel);

            public delegate int grpcsharp_sizeof_grpc_event_delegate();

            public delegate CompletionQueueSafeHandle grpcsharp_completion_queue_create_delegate();
            public delegate void grpcsharp_completion_queue_shutdown_delegate(CompletionQueueSafeHandle cq);
            public delegate CompletionQueueEvent grpcsharp_completion_queue_next_delegate(CompletionQueueSafeHandle cq);
#if UNITY_EDITOR
            public delegate CompletionQueueEvent grpcsharp_completion_queue_next_debuggable_delegate(CompletionQueueSafeHandle cq, MagicForDebugDelegate callback);
#endif
            public delegate CompletionQueueEvent grpcsharp_completion_queue_pluck_delegate(CompletionQueueSafeHandle cq, IntPtr tag);
            public delegate void grpcsharp_completion_queue_destroy_delegate(IntPtr cq);

            public delegate void gprsharp_free_delegate(IntPtr ptr);

            public delegate MetadataArraySafeHandle grpcsharp_metadata_array_create_delegate(UIntPtr capacity);
            public delegate void grpcsharp_metadata_array_add_delegate(MetadataArraySafeHandle array, string key, byte[] value, UIntPtr valueLength);
            public delegate UIntPtr grpcsharp_metadata_array_count_delegate(IntPtr metadataArray);
            public delegate IntPtr grpcsharp_metadata_array_get_key_delegate(IntPtr metadataArray, UIntPtr index, out UIntPtr keyLength);
            public delegate IntPtr grpcsharp_metadata_array_get_value_delegate(IntPtr metadataArray, UIntPtr index, out UIntPtr valueLength);
            public delegate void grpcsharp_metadata_array_destroy_full_delegate(IntPtr array);

            public delegate void grpcsharp_redirect_log_delegate(GprLogDelegate callback);

            public delegate CallCredentialsSafeHandle grpcsharp_metadata_credentials_create_from_plugin_delegate(NativeMetadataInterceptor interceptor);
            public delegate void grpcsharp_metadata_credentials_notify_from_plugin_delegate(IntPtr callbackPtr, IntPtr userData, MetadataArraySafeHandle metadataArray, StatusCode statusCode, string errorDetails);

            public delegate void grpcsharp_server_credentials_release_delegate(IntPtr credentials);

            public delegate void grpcsharp_server_destroy_delegate(IntPtr server);

            public delegate AuthContextSafeHandle grpcsharp_call_auth_context_delegate(CallSafeHandle call);
            public delegate IntPtr grpcsharp_auth_context_peer_identity_property_name_delegate(AuthContextSafeHandle authContext);  // returns const char*
            public delegate AuthContextSafeHandle.NativeAuthPropertyIterator grpcsharp_auth_context_property_iterator_delegate(AuthContextSafeHandle authContext);
            public delegate IntPtr grpcsharp_auth_property_iterator_next_delegate(ref AuthContextSafeHandle.NativeAuthPropertyIterator iterator);  // returns const auth_property*
            public delegate void grpcsharp_auth_context_release_delegate(IntPtr authContext);

            public delegate Timespec gprsharp_now_delegate(ClockType clockType);
            public delegate Timespec gprsharp_inf_future_delegate(ClockType clockType);
            public delegate Timespec gprsharp_inf_past_delegate(ClockType clockType);

            public delegate Timespec gprsharp_convert_clock_type_delegate(Timespec t, ClockType targetClock);
            public delegate int gprsharp_sizeof_timespec_delegate();

            public delegate CallError grpcsharp_test_callback_delegate([MarshalAs(UnmanagedType.FunctionPtr)] OpCompletionDelegate callback);
            public delegate IntPtr grpcsharp_test_nop_delegate(IntPtr ptr);
        }

        static class NativeCalls
        {
            [DllImport(pluginName)]
            internal static extern void grpcsharp_init();

            [DllImport(pluginName)]
            internal static extern void grpcsharp_shutdown();

            [DllImport(pluginName)]
            internal static extern IntPtr grpcsharp_version_string();  // returns not-owned const char*


            [DllImport(pluginName)]
            internal static extern BatchContextSafeHandle grpcsharp_batch_context_create();

            [DllImport(pluginName)]
            internal static extern IntPtr grpcsharp_batch_context_recv_initial_metadata(BatchContextSafeHandle ctx);

            [DllImport(pluginName)]
            internal static extern IntPtr grpcsharp_batch_context_recv_message_length(BatchContextSafeHandle ctx);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_batch_context_recv_message_to_buffer(BatchContextSafeHandle ctx, byte[] buffer, UIntPtr bufferLen);

            [DllImport(pluginName)]
            internal static extern StatusCode grpcsharp_batch_context_recv_status_on_client_status(BatchContextSafeHandle ctx);

            [DllImport(pluginName)]
            internal static extern IntPtr grpcsharp_batch_context_recv_status_on_client_details(BatchContextSafeHandle ctx, out UIntPtr detailsLength);

            [DllImport(pluginName)]
            internal static extern IntPtr grpcsharp_batch_context_recv_status_on_client_trailing_metadata(BatchContextSafeHandle ctx);

            [DllImport(pluginName)]
            internal static extern int grpcsharp_batch_context_recv_close_on_server_cancelled(BatchContextSafeHandle ctx);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_batch_context_destroy(IntPtr ctx);


            [DllImport(pluginName)]
            internal static extern void grpcsharp_request_call_context_destroy(IntPtr ctx);

            [DllImport(pluginName)]
            internal static extern CallCredentialsSafeHandle grpcsharp_composite_call_credentials_create(CallCredentialsSafeHandle creds1, CallCredentialsSafeHandle creds2);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_call_credentials_release(IntPtr credentials);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_cancel(CallSafeHandle call);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_cancel_with_status(CallSafeHandle call, StatusCode status, string description);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_start_unary(CallSafeHandle call, BatchContextSafeHandle ctx, byte[] sendBuffer, UIntPtr sendBufferLen, WriteFlags writeFlags, MetadataArraySafeHandle metadataArray, CallFlags metadataFlags);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_start_client_streaming(CallSafeHandle call, BatchContextSafeHandle ctx, MetadataArraySafeHandle metadataArray, CallFlags metadataFlags);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_start_server_streaming(CallSafeHandle call, BatchContextSafeHandle ctx, byte[] sendBuffer, UIntPtr sendBufferLen, WriteFlags writeFlags, MetadataArraySafeHandle metadataArray, CallFlags metadataFlags);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_start_duplex_streaming(CallSafeHandle call, BatchContextSafeHandle ctx, MetadataArraySafeHandle metadataArray, CallFlags metadataFlags);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_send_message(CallSafeHandle call, BatchContextSafeHandle ctx, byte[] sendBuffer, UIntPtr sendBufferLen, WriteFlags writeFlags, bool sendEmptyInitialMetadata);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_send_close_from_client(CallSafeHandle call, BatchContextSafeHandle ctx);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_send_status_from_server(CallSafeHandle call, BatchContextSafeHandle ctx, StatusCode statusCode, byte[] statusMessage, UIntPtr statusMessageLen, MetadataArraySafeHandle metadataArray, bool sendEmptyInitialMetadata, byte[] optionalSendBuffer, UIntPtr optionalSendBufferLen, WriteFlags writeFlags);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_recv_message(CallSafeHandle call, BatchContextSafeHandle ctx);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_recv_initial_metadata(CallSafeHandle call, BatchContextSafeHandle ctx);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_start_serverside(CallSafeHandle call, BatchContextSafeHandle ctx);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_send_initial_metadata(CallSafeHandle call, BatchContextSafeHandle ctx, MetadataArraySafeHandle metadataArray);

            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_call_set_credentials(CallSafeHandle call, CallCredentialsSafeHandle credentials);

            [DllImport(pluginName)]
            internal static extern CStringSafeHandle grpcsharp_call_get_peer(CallSafeHandle call);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_call_destroy(IntPtr call);


            [DllImport(pluginName)]
            internal static extern ChannelArgsSafeHandle grpcsharp_channel_args_create(UIntPtr numArgs);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_channel_args_set_string(ChannelArgsSafeHandle args, UIntPtr index, string key, string value);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_channel_args_set_integer(ChannelArgsSafeHandle args, UIntPtr index, string key, int value);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_channel_args_destroy(IntPtr args);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_override_default_ssl_roots(string pemRootCerts);

            [DllImport(pluginName)]
            internal static extern ChannelCredentialsSafeHandle grpcsharp_ssl_credentials_create(string pemRootCerts, string keyCertPairCertChain, string keyCertPairPrivateKey);

            [DllImport(pluginName)]
            internal static extern ChannelCredentialsSafeHandle grpcsharp_composite_channel_credentials_create(ChannelCredentialsSafeHandle channelCreds, CallCredentialsSafeHandle callCreds);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_channel_credentials_release(IntPtr credentials);

            [DllImport(pluginName)]
            internal static extern ChannelSafeHandle grpcsharp_insecure_channel_create(string target, ChannelArgsSafeHandle channelArgs);

            [DllImport(pluginName)]
            internal static extern ChannelSafeHandle grpcsharp_secure_channel_create(ChannelCredentialsSafeHandle credentials, string target, ChannelArgsSafeHandle channelArgs);

            [DllImport(pluginName)]
            internal static extern CallSafeHandle grpcsharp_channel_create_call(ChannelSafeHandle channel, CallSafeHandle parentCall, ContextPropagationFlags propagationMask, CompletionQueueSafeHandle cq, string method, string host, Timespec deadline);

            [DllImport(pluginName)]
            internal static extern ChannelState grpcsharp_channel_check_connectivity_state(ChannelSafeHandle channel, int tryToConnect);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_channel_watch_connectivity_state(ChannelSafeHandle channel, ChannelState lastObservedState, Timespec deadline, CompletionQueueSafeHandle cq, BatchContextSafeHandle ctx);

            [DllImport(pluginName)]
            internal static extern CStringSafeHandle grpcsharp_channel_get_target(ChannelSafeHandle call);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_channel_destroy(IntPtr channel);


            [DllImport(pluginName)]
            internal static extern int grpcsharp_sizeof_grpc_event();


            [DllImport(pluginName)]
            internal static extern CompletionQueueSafeHandle grpcsharp_completion_queue_create();

            [DllImport(pluginName)]
            internal static extern void grpcsharp_completion_queue_shutdown(CompletionQueueSafeHandle cq);

            [DllImport(pluginName)]
            internal static extern CompletionQueueEvent grpcsharp_completion_queue_next(CompletionQueueSafeHandle cq);

#if UNITY_EDITOR
            [DllImport(pluginName)]
            internal static extern CompletionQueueEvent grpcsharp_completion_queue_next_debuggable(CompletionQueueSafeHandle cq, [MarshalAs(UnmanagedType.FunctionPtr)] MagicForDebugDelegate callback);
#endif

            [DllImport(pluginName)]
            internal static extern CompletionQueueEvent grpcsharp_completion_queue_pluck(CompletionQueueSafeHandle cq, IntPtr tag);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_completion_queue_destroy(IntPtr cq);


            [DllImport(pluginName)]
            internal static extern void gprsharp_free(IntPtr ptr);


            [DllImport(pluginName)]
            internal static extern MetadataArraySafeHandle grpcsharp_metadata_array_create(UIntPtr capacity);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_metadata_array_add(MetadataArraySafeHandle array, string key, byte[] value, UIntPtr valueLength);

            [DllImport(pluginName)]
            internal static extern UIntPtr grpcsharp_metadata_array_count(IntPtr metadataArray);

            [DllImport(pluginName)]
            internal static extern IntPtr grpcsharp_metadata_array_get_key(IntPtr metadataArray, UIntPtr index, out UIntPtr keyLength);

            [DllImport(pluginName)]
            internal static extern IntPtr grpcsharp_metadata_array_get_value(IntPtr metadataArray, UIntPtr index, out UIntPtr valueLength);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_metadata_array_destroy_full(IntPtr array);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_redirect_log(GprLogDelegate callback);

            [DllImport(pluginName)]
            internal static extern CallCredentialsSafeHandle grpcsharp_metadata_credentials_create_from_plugin(NativeMetadataInterceptor interceptor);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_metadata_credentials_notify_from_plugin(IntPtr callbackPtr, IntPtr userData, MetadataArraySafeHandle metadataArray, StatusCode statusCode, string errorDetails);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_server_credentials_release(IntPtr credentials);

            [DllImport(pluginName)]
            internal static extern void grpcsharp_server_destroy(IntPtr server);


            [DllImport(pluginName)]
            internal static extern AuthContextSafeHandle grpcsharp_call_auth_context(CallSafeHandle call);

            [DllImport(pluginName)]
            internal static extern IntPtr grpcsharp_auth_context_peer_identity_property_name(AuthContextSafeHandle authContext);  // returns const char*

            [DllImport(pluginName)]
            internal static extern AuthContextSafeHandle.NativeAuthPropertyIterator grpcsharp_auth_context_property_iterator(AuthContextSafeHandle authContext);

            [DllImport(pluginName)]
            internal static extern IntPtr grpcsharp_auth_property_iterator_next(ref AuthContextSafeHandle.NativeAuthPropertyIterator iterator);  // returns const auth_property*

            [DllImport(pluginName)]
            internal static extern void grpcsharp_auth_context_release(IntPtr authContext);


            [DllImport(pluginName)]
            internal static extern Timespec gprsharp_now(ClockType clockType);

            [DllImport(pluginName)]
            internal static extern Timespec gprsharp_inf_future(ClockType clockType);

            [DllImport(pluginName)]
            internal static extern Timespec gprsharp_inf_past(ClockType clockType);

            [DllImport(pluginName)]
            internal static extern Timespec gprsharp_convert_clock_type(Timespec t, ClockType targetClock);

            [DllImport(pluginName)]
            internal static extern int gprsharp_sizeof_timespec();


            [DllImport(pluginName)]
            internal static extern CallError grpcsharp_test_callback([MarshalAs(UnmanagedType.FunctionPtr)] OpCompletionDelegate callback);

            [DllImport(pluginName)]
            internal static extern IntPtr grpcsharp_test_nop(IntPtr ptr);
        }
    }
}
