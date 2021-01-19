using System;
using System.Diagnostics;

namespace Benchmark.ClientLib
{
    public class Statistics : IDisposable
    {
        public string Name { get; }
        public DateTime Begin { get; }
        public DateTime End { get; private set; }
        public TimeSpan Duration { get; private set; }
        public TimeSpan Elapsed => _stopwatch.Elapsed;
        
        private readonly Stopwatch _stopwatch;
        public Statistics(string name = "")
        {
            Name = name;
            Begin = DateTime.UtcNow;
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
            End = DateTime.UtcNow;
            Duration = Elapsed;
            _stopwatch.Stop();

            Console.WriteLine($" * Elapsed({Name}): {Duration.TotalMilliseconds}ms");
        }
    }
}
