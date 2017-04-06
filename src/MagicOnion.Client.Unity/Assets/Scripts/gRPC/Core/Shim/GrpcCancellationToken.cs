// Shim of CancellationToken, CancellationTokenSource

using System;
using System.Collections.Generic;
using System.Threading;

namespace Grpc.Core
{
    public class GrpcCancellationTokenSource
    {
        int state = 1; // 1 = normal, 2 = alreadyCanceled
        object gate = new object();
        List<Action> callbackActions;
        List<Action> callbackActionsLast;

        public bool CanBeCanceled
        {
            get { return state > 0; }
        }

        public bool IsCancellationRequested
        {
            get { return state >= 2; }
        }

        public GrpcCancellationToken Token
        {
            get
            {
                return new GrpcCancellationToken(this);
            }
        }

        public void Cancel()
        {
            if (Interlocked.CompareExchange(ref state, 2, 1) == 1)
            {
                lock (gate)
                {
                    if (callbackActions != null)
                    {
                        for (int i = 0; i < callbackActions.Count; i++)
                        {
                            callbackActions[i].Invoke();
                        }
                    }
                    if (callbackActionsLast != null)
                    {
                        for (int i = 0; i < callbackActionsLast.Count; i++)
                        {
                            callbackActionsLast[i].Invoke();
                        }
                    }
                }
            }
        }

        internal void InternalRegister(Action callback)
        {
            if (!IsCancellationRequested)
            {
                lock (gate)
                {
                    if (callbackActions == null)
                    {
                        callbackActions = new List<Action>();
                    }
                    callbackActions.Add(callback);
                }
            }
            else
            {
                callback();
            }
        }

        // second priority
        internal void InternalRegisterLast(Action callback)
        {
            if (!IsCancellationRequested)
            {
                lock (gate)
                {
                    if (callbackActionsLast == null)
                    {
                        callbackActionsLast = new List<Action>();
                    }
                    callbackActionsLast.Add(callback);
                }
            }
            else
            {
                callback();
            }
        }
    }

    public struct GrpcCancellationToken
    {
        public static GrpcCancellationToken None = default(GrpcCancellationToken);

        readonly GrpcCancellationTokenSource source;

        public bool CanBeCanceled
        {
            get { return source != null && source.CanBeCanceled; }
        }

        public bool IsCancellationRequested
        {
            get { return source != null && source.IsCancellationRequested; }
        }

        public GrpcCancellationToken(GrpcCancellationTokenSource source)
        {
            this.source = source;
        }

        public void ThrowIfCancellationRequested()
        {
            if (IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }

        public void Register(Action callback)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (!CanBeCanceled) return;

            source.InternalRegister(callback);
        }

        public void RegisterLast(Action callback)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (!CanBeCanceled) return;

            source.InternalRegisterLast(callback);
        }
    }
}
