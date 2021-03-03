using System;
using System.Threading;

namespace Benchmark.ClientLib.Scenarios
{
    public class ScenarioBase
    {
        public Exception Exception => _exception;
        private Exception _exception = null;

        public int Error => _error;
        private int _error = 0;

        public bool FailFast { get; }

        public ScenarioBase(bool failFast) => FailFast = failFast;

        public void IncrementError()
        {
            Interlocked.Increment(ref _error);
        }
        public void ResetError()
        {
            Interlocked.Exchange(ref _error, 0);
        }

        public void PostException(Exception ex)
        {
            if (Interlocked.CompareExchange(ref _exception, ex, null) == null)
            {
                if (_exception == null)
                {
                    _exception = ex;
                }
            }
        }
    }
}
