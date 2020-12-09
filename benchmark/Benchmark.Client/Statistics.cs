using System;
using System.Diagnostics;

namespace Benchmark.Client
{
    public class Statistics : IDisposable
    {
        public TimeSpan Duration { get; private set; }
        public TimeSpan Current => _stopwatch.Elapsed;
        
        private readonly Stopwatch _stopwatch;
        public Statistics()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public void Pause()
        {
            if (_stopwatch.IsRunning)
            {
                _stopwatch.Stop();
            }
        }

        public void Restart()
        {
            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
            }
        }

        public void Dispose()
        {
            Duration = Current;
            _stopwatch.Stop();

            Console.WriteLine($"Elapsed time: {Duration.TotalMilliseconds}ms");
        }
    }
}
