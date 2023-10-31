using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if MAGICONION_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#endif
#if MAGICONION_USE_GRPC_CCORE
using Channel = Grpc.Core.Channel;
#else
using Grpc.Net.Client;
#endif
using MagicOnion.Client;
using UnityEditor;
using UnityEngine;

namespace MagicOnion.Unity.Editor
{
    public class GrpcChannelProviderMonitor
    {
        private readonly Dictionary<int, bool> _stackTraceFoldout = new Dictionary<int, bool>();
        private readonly Dictionary<int, bool> _channelOptionsFoldout = new Dictionary<int, bool>();
        private readonly Dictionary<GrpcChannelx, DataRatePoints> _historyByChannel = new Dictionary<GrpcChannelx, DataRatePoints>();
        private readonly IGrpcChannelProvider _provider;
        private DateTime _lastUpdatedAt;

        public GrpcChannelProviderMonitor(IGrpcChannelProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public bool TryUpdate()
        {
            if ((DateTime.Now - _lastUpdatedAt).TotalSeconds >= 1)
            {
                foreach (var channel in _provider.GetAllChannels())
                {
                    if (!_historyByChannel.TryGetValue(channel, out var history))
                    {
                        _historyByChannel[channel] = history = new DataRatePoints();
                    }

                    var diagInfo = ((IGrpcChannelxDiagnosticsInfo) channel);
                    history.AddValues(diagInfo.Stats.ReceiveBytesPerSecond, diagInfo.Stats.SentBytesPerSecond);
                }

                _lastUpdatedAt = DateTime.Now;
                return true;
            }

            return false;
        }

        public void DrawChannels()
        {
            var channels = _provider.GetAllChannels();
            if (!channels.Any())
            {
                EditorGUILayout.HelpBox("The application has no gRPC channel yet.", MessageType.Info);
                return;
            }

            foreach (var channel in channels)
            {
                var diagInfo = ((IGrpcChannelxDiagnosticsInfo)channel);

                if (!_historyByChannel.TryGetValue(channel, out var history))
                {
                    _historyByChannel[channel] = history = new DataRatePoints();
                }

                using (new Section(() =>
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
#if MAGICONION_USE_GRPC_CCORE
                        if (diagInfo.UnderlyingChannel is Channel grpcCCoreChannel)
                        {
                            EditorGUILayout.LabelField($"Channel:  {channel.Id} ({channel.Target}; State={grpcCCoreChannel.State})", EditorStyles.boldLabel);
                        }
                        else
#endif
                        {
                            EditorGUILayout.LabelField($"Channel:  {channel.Id} ({channel.Target})", EditorStyles.boldLabel);
                        }
                        if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
                        {
                            var menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Disconnect"), false, x => { ((GrpcChannelx)x).Dispose(); }, channel);
                            menu.ShowAsContext();
                        }
                    }
                }))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Total Received Bytes", diagInfo.Stats.ReceivedBytes.ToString("#,0") + " bytes");
                        EditorGUILayout.LabelField("Total Sent Bytes", diagInfo.Stats.SentBytes.ToString("#,0") + " bytes");
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Received Bytes/Sec", diagInfo.Stats.ReceiveBytesPerSecond.ToString("#,0") + " bytes/s");
                        EditorGUILayout.LabelField("Sent Bytes/Sec", diagInfo.Stats.SentBytesPerSecond.ToString("#,0") + " bytes/s");
                    }
                    DrawChart(channel, history);

                    var streamingHubs = ((IMagicOnionAwareGrpcChannel)channel).GetAllManagedStreamingHubs();
                    if (streamingHubs.Any())
                    {
                        using (new Section("StreamingHubs"))
                        {
                            foreach (var streamingHub in streamingHubs)
                            {
                                EditorGUILayout.LabelField(streamingHub.StreamingHubType.FullName);
                                EditorGUI.indentLevel++;
                                EditorGUILayout.LabelField(streamingHub.Client.GetType().FullName);
                                EditorGUI.indentLevel--;
                            }
                        }
                    }

                    if (_stackTraceFoldout[channel.Id] = EditorGUILayout.Foldout(_stackTraceFoldout.TryGetValue(channel.Id, out var foldout) ? foldout : false, "StackTrace"))
                    {
                        var reducedStackTrace = string.Join("\n", diagInfo.StackTrace
                            .Split('\n')
                            .SkipWhile(x => x.StartsWith($"  at {typeof(GrpcChannelx).FullName}"))
                            .Where(x => !x.StartsWith("  at System.Runtime.CompilerServices."))
                            .Where(x => !x.StartsWith("  at System.Threading.ExecutionContext."))
                            .Where(x => !x.StartsWith("  at Cysharp.Threading.Tasks.CompilerServices."))
                            .Where(x => !x.StartsWith("  at Cysharp.Threading.Tasks.AwaiterActions."))
                            .Where(x => !x.StartsWith("  at Cysharp.Threading.Tasks.UniTask"))
                            .Where(x => !x.StartsWith("  at MagicOnion.Unity.GrpcChannelProviderExtensions."))
                        );
                        EditorGUILayout.TextArea(reducedStackTrace, GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth - 40));
                    }

                    if (_channelOptionsFoldout[channel.Id] = EditorGUILayout.Foldout(_channelOptionsFoldout.TryGetValue(channel.Id, out var channelOptionsFoldout) ? channelOptionsFoldout : false, "ChannelOptions"))
                    {
                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            var prevLabelWidth = EditorGUIUtility.labelWidth;
                            EditorGUIUtility.labelWidth = 350;

                            foreach (var keyValue in diagInfo.ChannelOptions.GetValues())
                            {
                                if (keyValue.Value is int intValue)
                                {
                                    EditorGUILayout.IntField(keyValue.Key, intValue);
                                }
                                else if (keyValue.Value is long longValue)
                                {
                                    EditorGUILayout.LongField(keyValue.Key, longValue);
                                }
                                else
                                {
                                    EditorGUILayout.TextField(keyValue.Key, keyValue.Value?.ToString() ?? "");
                                }
                            }

                            EditorGUIUtility.labelWidth = prevLabelWidth;
                        }
                    }
                }
            }
        }

        private void DrawChart(GrpcChannelx channel, DataRatePoints history)
        {
            // Draw Chart
            var prevHandlesColor = Handles.color;
            {
                var rect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(GUILayoutUtility.GetLastRect().width, 100));
                Handles.DrawSolidRectangleWithOutline(rect, Color.black, Color.gray);

                var drawableRect = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
                var maxCount = 60;
                var maxY = DataUnit.GetScaleMaxValue(Math.Max(history.ReceivedValues.DefaultIfEmpty().Max(), history.SentValues.DefaultIfEmpty().Max()));
                var dx = drawableRect.width / (float)(maxCount - 1);
                var dy = drawableRect.height / (float)maxY;

                // Grid (9 lines)
                Handles.color = new Color(0.05f, 0.25f, 0.05f);
                for (var i = 1; i < 10; i++)
                {
                    var yTop = drawableRect.y + (drawableRect.height / 10) * i;
                    Handles.DrawLine(new Vector3(drawableRect.x, yTop), new Vector3(drawableRect.xMax, yTop));
                }

                // Label
                Handles.Label(new Vector3(drawableRect.x, drawableRect.y), DataUnit.Humanize(maxY) + " bytes/sec");

                // Values
                Span<int> buffer = stackalloc int[60];
                var points = new List<Vector3>();
                {
                    var values = history.ReceivedValues.ToSpan(buffer);
                    for (var i = 0; i < Math.Min(maxCount, values.Length); i++)
                    {
                        var p = new Vector3(drawableRect.x + (drawableRect.width - (dx * i)), drawableRect.yMax - (dy * values[values.Length - i - 1]));
                        points.Add(p);
                    }

                    Handles.color = new Color(0.33f, 0.55f, 0.33f);
                    Handles.DrawAAPolyLine(4f, points.ToArray());
                }
                points.Clear();
                {
                    var values = history.SentValues.ToSpan(buffer);
                    for (var i = 0; i < Math.Min(maxCount, values.Length); i++)
                    {
                        var p = new Vector3(drawableRect.x + (drawableRect.width - (dx * i)), drawableRect.yMax - (dy * values[values.Length - i - 1]));
                        points.Add(p);
                    }

                    Handles.color = new Color(0.2f, 0.33f, 0.2f);
                    Handles.DrawAAPolyLine(2f, points.ToArray());

                }
            }
            Handles.color = prevHandlesColor;
        }

        private class DataRatePoints
        {
            public RingBuffer<int> ReceivedValues { get; }
            public RingBuffer<int> SentValues { get; }

            public DataRatePoints()
            {
                ReceivedValues = new RingBuffer<int>(60);
                SentValues = new RingBuffer<int>(60);
            }

            public void AddValues(int receivedBytesPerSecond, int sentBytesPerSecond)
            {
                ReceivedValues.Add(receivedBytesPerSecond);
                SentValues.Add(sentBytesPerSecond);
            }
        }

        private struct Section : IDisposable
        {
            public Section(string label)
            {
                if (!string.IsNullOrWhiteSpace(label))
                {
                    EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                }
                EditorGUI.indentLevel++;
            }

            public Section(Action action)
            {
                action();
                EditorGUI.indentLevel++;
            }

            public void Dispose()
            {
                EditorGUI.indentLevel--;
                EditorGUILayout.Separator();
            }
        }

        private class RingBuffer<T> : IEnumerable<T>
        {
            private readonly T[] _buffer;
            private int _index = 0;

            private bool IsFull => _index > _buffer.Length;
            private int StartIndex => IsFull ? _index % _buffer.Length : 0;
            public int Length => IsFull ? _buffer.Length : _index;

            public RingBuffer(int bufferSize)
            {
                _buffer = new T[bufferSize];
            }

            public void Add(T item)
            {
                _buffer[_index++ % _buffer.Length] = item;
            }

            public Span<T> ToSpan(Span<T> buffer)
            {
                var index = 0;
                foreach (var value in this)
                {
                    buffer[index++] = value;
                }

                return buffer.Slice(0, index);
            }

            public Enumerator GetEnumerator()
                => new Enumerator(_buffer, _index, Length);

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
                => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            public struct Enumerator : IEnumerator<T>
            {
                private readonly T[] _buffer;
                private readonly int _start;
                private readonly int _length;

                private int _current;

                public Enumerator(T[] buffer, int start, int length)
                {
                    _buffer = buffer;
                    _start = start;
                    _length = length;

                    _current = -1;
                }

                public T Current => _buffer[(_start + _current) % _length];

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    _current++;
                    return (_current < _length);
                }

                public void Reset()
                {
                    _current = -1;
                }
            }
        }

        private class DataUnit
        {
            private static readonly long[] _scales = new long[]
            {
                1_000, // 1 K
                10_000, // 10 K
                100_000, // 100 K
                500_000, // 500 K
                1_000_000, // 1 M
                2_000_000, // 2 M
                10_000_000, // 10 M
                20_000_000, // 20 M
                100_000_000, // 100 M
                200_000_000, // 200 M
                500_000_000, // 500 M
                1_000_000_000, // 1 G
                10_000_000_000, // 10 G
                20_000_000_000, // 20 G
                50_000_000_000, // 10 G
                100_000_000_000, // 100 G
            };

            public static long GetScaleMaxValue(long value)
            {
                foreach (var scale in _scales)
                {
                    if (value < scale)
                    {
                        return scale;
                    }
                }

                return value;
            }

            public static string Humanize(long value)
            {
                if (value <= 1_000)
                {
                    return value.ToString("#,0");
                }
                else if (value <= 1_000_000)
                {
                    return (value / 1_000).ToString("#,0") + " K"; // K
                }
                else if (value <= 1_000_000_000)
                {
                    return (value / 1_000_000).ToString("#,0") + " M"; // M
                }
                else if (value <= 1_000_000_000_000)
                {
                    return (value / 1_000_000_000).ToString("#,0") + " G"; // G
                }
                else
                {
                    return (value / 1_000_000_000_000).ToString("#,0") + " T"; // T
                }
            }
        }
    }
}
