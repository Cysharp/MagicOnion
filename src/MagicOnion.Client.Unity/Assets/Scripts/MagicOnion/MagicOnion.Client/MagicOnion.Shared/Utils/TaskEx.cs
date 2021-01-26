using System.Threading.Tasks;

namespace MagicOnion
{
    internal static class TaskEx
    {
        public static readonly Task CompletedTask =
#if net45
            Task.FromResult(0);
#else
            Task.CompletedTask;
#endif

    }
}
