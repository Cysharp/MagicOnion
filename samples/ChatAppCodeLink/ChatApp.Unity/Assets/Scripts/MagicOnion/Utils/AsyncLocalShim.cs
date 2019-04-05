#if net45
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Threading
{
    /// <summary>
    /// Polyfill for AsyncLocal functionality added in NETSTANDARD1.3 and .NET 4.6
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value to encapsulate and follow.
    /// </typeparam>
    internal class AsyncLocal<T>
    {
        private readonly string _name = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Gets or set the value to flow with <see cref="ExecutionContext"/>.
        /// </summary>
        public T Value
        {
            get
            {
                if (CallContext.LogicalGetData(_name) is ObjectHandle slot)
                    return (T)slot.Unwrap();
                return default(T);
            }
            set
            {
                // Mimic the implementation of AsyncLocal<T>
                var executionContext = Thread.CurrentThread.GetMutableExecutionContext();
                var logicalCallContext = executionContext.GetLogicalCallContext();
                var datastore = logicalCallContext.GetDatastore();
                var datastoreCopy = datastore == null ? new Hashtable() : new Hashtable(datastore);
                var slot = new ObjectHandle(value);
                datastoreCopy[_name] = slot;
                logicalCallContext.SetDatastore(datastoreCopy);
            }
        }
    }

    internal static class ThreadExtensions
    {
        private static readonly Func<Thread, ExecutionContext> _getMutableExecutionContextFunc;

        static ThreadExtensions()
        {
            var getMutableExecutionContextMethodInfo = typeof(Thread).GetMethod("GetMutableExecutionContext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var instanceParameterExpression = Expression.Parameter(typeof(Thread));
            var functionCallExpression = Expression.Call(instanceParameterExpression, getMutableExecutionContextMethodInfo);
            var lambdaExpression = Expression.Lambda<Func<Thread, ExecutionContext>>(functionCallExpression, instanceParameterExpression);
            _getMutableExecutionContextFunc = lambdaExpression.Compile();
        }

        private static MethodInfo GetMutableExecutionContextMethodInfo =
            typeof(Thread).GetMethod("GetMutableExecutionContext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public static ExecutionContext GetMutableExecutionContext(this Thread thread)
            => _getMutableExecutionContextFunc(thread);
    }

    internal static class LogicalCallContextExtensions
    {
        private static readonly Func<LogicalCallContext, Hashtable> _getDatastoreFunc;
        private static readonly Func<LogicalCallContext, Hashtable, bool> _setDatastoreFunc;

        static LogicalCallContextExtensions()
        {
            var datastoreFieldInfo = typeof(LogicalCallContext).GetField("m_Datastore", BindingFlags.Instance | BindingFlags.NonPublic);
            var instanceParameterExpression = Expression.Parameter(typeof(LogicalCallContext));
            var memberAccessExpression = Expression.MakeMemberAccess(instanceParameterExpression, datastoreFieldInfo);
            var getLambdaExpression = Expression.Lambda<Func<LogicalCallContext, Hashtable>>(memberAccessExpression, instanceParameterExpression);
            _getDatastoreFunc = getLambdaExpression.Compile();
            var valueParameterExpression = Expression.Parameter(typeof(Hashtable));
            var assignmentExpression = Expression.Assign(memberAccessExpression, valueParameterExpression);
            var setFunctionBody = Expression.Block(assignmentExpression, Expression.Constant(true));
            var setLambdaExpression = Expression.Lambda<Func<LogicalCallContext, Hashtable, bool>>(setFunctionBody, instanceParameterExpression, valueParameterExpression);
            _setDatastoreFunc = setLambdaExpression.Compile();
        }

        public static Hashtable GetDatastore(this LogicalCallContext context)
            => _getDatastoreFunc(context);

        public static void SetDatastore(this LogicalCallContext context, Hashtable datastore)
            => _setDatastoreFunc(context, datastore);
    }

    internal static class ExecutionContextExtensions
    {
        private static readonly Func<ExecutionContext, LogicalCallContext> _getLogicalCallContextFunc;

        static ExecutionContextExtensions()
        {
            var logicalCallContextPropertyInfo = typeof(ExecutionContext).GetProperty("LogicalCallContext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var instanceParameterExpression = Expression.Parameter(typeof(ExecutionContext));
            var memberAccessExpression = Expression.MakeMemberAccess(instanceParameterExpression, logicalCallContextPropertyInfo);
            var lambdaExpression = Expression.Lambda<Func<ExecutionContext, LogicalCallContext>>(memberAccessExpression, instanceParameterExpression);
            _getLogicalCallContextFunc = lambdaExpression.Compile();
        }

        public static LogicalCallContext GetLogicalCallContext(this ExecutionContext executionContext)
            => _getLogicalCallContextFunc(executionContext);
    }
}

#endif