using System;
using System.Runtime.CompilerServices;
using System.Security;

namespace MagicOnion.CompilerServices
{
    public struct AsyncUnaryResultMethodBuilder<T>
    {
        private AsyncTaskMethodBuilder<T> methodBuilder;
        private T result;
        private bool haveResult;
        private bool useBuilder;

        public static AsyncUnaryResultMethodBuilder<T> Create()
        {
            return new AsyncUnaryResultMethodBuilder<T>() { methodBuilder = AsyncTaskMethodBuilder<T>.Create() };
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            methodBuilder.Start(ref stateMachine); // will provide the right ExecutionContext semantics
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            methodBuilder.SetStateMachine(stateMachine);
        }

        public void SetResult(T result)
        {
            if (useBuilder)
            {
                methodBuilder.SetResult(result);
            }
            else
            {
                this.result = result;
                haveResult = true;
            }
        }

        public void SetException(Exception exception)
        {
            methodBuilder.SetException(exception);
        }

        public UnaryResult<T> Task
        {
            get
            {
                if (haveResult)
                {
                    return new UnaryResult<T>(result);
                }
                else
                {
                    useBuilder = true;
                    return new UnaryResult<T>(methodBuilder.Task);
                }
            }
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            useBuilder = true;
            methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            useBuilder = true;
            methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }
    }
}