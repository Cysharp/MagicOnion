using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion.Server.CompilerServices
{
    public struct AsyncUnaryResultMethodBuilder<TResponse>
    {
        AsyncTaskMethodBuilder<TResponse> builder;

        public UnaryResult<TResponse> UnaryResult
        {
            get
            {
                var result = new UnaryResult<TResponse>();
                return result;
            }
        }

        public static AsyncUnaryResultMethodBuilder<TResponse> Create()
        {
            return new AsyncUnaryResultMethodBuilder<TResponse>() { builder = AsyncTaskMethodBuilder<TResponse>.Create() };
        }

        public void SetResult(TResponse result)
        {
            builder.SetResult(result);
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }

        public void SetException(Exception exception)
        {
            builder.SetException(exception);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            builder.SetStateMachine(stateMachine);
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            builder.Start(ref stateMachine);
        }
    }
}
