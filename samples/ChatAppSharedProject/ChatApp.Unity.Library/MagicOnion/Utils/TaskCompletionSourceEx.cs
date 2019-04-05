using System;
using System.Threading.Tasks;

namespace MagicOnion.Utils
{
    internal interface ITaskCompletion
    {
        bool TrySetException(Exception ex);
        bool TrySetCanceled();
    }

    internal class TaskCompletionSourceEx<T> : TaskCompletionSource<T>, ITaskCompletion
    {
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