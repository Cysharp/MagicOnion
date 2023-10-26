using System;
using System.Threading.Tasks;

namespace MagicOnion.Client.Internal.Threading.Tasks
{
    internal interface ITaskCompletion
    {
        bool TrySetException(Exception ex);
        bool TrySetCanceled();
    }

    internal class TaskCompletionSourceEx<T> : TaskCompletionSource<T>, ITaskCompletion
    {
        public TaskCompletionSourceEx()
        { }
        public TaskCompletionSourceEx(TaskCreationOptions options) : base(options)
        { }

        bool ITaskCompletion.TrySetCanceled()
        {
            return this.TrySetCanceled();
        }

        bool ITaskCompletion.TrySetException(Exception ex)
        {
            return this.TrySetException(ex);
        }
    }
}
