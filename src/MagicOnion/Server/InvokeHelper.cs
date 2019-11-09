using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    internal class InvokeHelper<TArg1, TDelegate>
        where TDelegate : Delegate
    {
        public Func<TArg1, TDelegate, ValueTask> Invoke;
        public TDelegate Next;

        private static readonly Func<InvokeHelper<TArg1, TDelegate>, TDelegate> InvokeNextFactory;

        static InvokeHelper()
        {
            var fieldInvoke = typeof(InvokeHelper<TArg1, TDelegate>).GetField("Invoke");
            var fieldNext = typeof(InvokeHelper<TArg1, TDelegate>).GetField("Next");
            var methodInvoke = typeof(Func<TArg1, TDelegate, ValueTask>).GetMethod("Invoke");

            if (RuntimeInformation.FrameworkDescription.StartsWith(".NET Core 4")) /* .NET Core 2.x returns ".NET Core 4.x.y.z" */
            {
                // HACK: If the app is running on .NET Core 2.2 or earlier, the runtime hides dynamic method in the stack trace.
                var method = new DynamicMethod("InvokeNext", typeof(ValueTask), new[] { typeof(InvokeHelper<TArg1, TDelegate>), typeof(TArg1) }, restrictedSkipVisibility: true);
                {
                    var il = method.GetILGenerator();

                    // invoke = arg0.Invoke;
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, fieldInvoke);

                    // next = arg0.Next;
                    // return invoke(arg1, next);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, fieldNext);
                    il.Emit(OpCodes.Callvirt, methodInvoke);
                    il.Emit(OpCodes.Ret);
                }

                InvokeNextFactory = (helper) => (TDelegate)method.CreateDelegate(typeof(TDelegate), helper);

            }
            else
            {
                // HACK: If the app is running on .NET Core 3.0 or later, the runtime hides `AggressiveInlining` method in the stack trace. (If the app is running on .NET Framework 4.x, This hack does not affect.)
                // https://github.com/dotnet/coreclr/blob/release/3.0/src/System.Private.CoreLib/shared/System/Diagnostics/StackTrace.cs#L343-L350
                InvokeNextFactory = (helper) =>
                {
                    var invokeNext = new Func<TArg1, ValueTask>(helper.InvokeNext);
                    return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), invokeNext.Target, invokeNext.Method);
                };
            }
        }

        public InvokeHelper(Func<TArg1, TDelegate, ValueTask> invoke, TDelegate next)
        {
            Invoke = invoke;
            Next = next;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        private ValueTask InvokeNext(TArg1 arg1) => Invoke(arg1, Next);

        public TDelegate GetDelegate() => InvokeNextFactory(this);
    }
}
