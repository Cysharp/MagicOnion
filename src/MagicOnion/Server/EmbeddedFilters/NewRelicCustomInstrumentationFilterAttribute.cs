using Grpc.Core;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// Note:this is test, may needs reference NewRelic.Api.Agent.

namespace MagicOnion.Server.EmbeddedFilters
{
    /// <summary>
    /// Marker of NewRelicAgent.
    /// Set Agent to config by C:\ProgramData\New Relic\.NET Agent\Extensions\CustomInstrumentation.xml
    /// See: https://discuss.newrelic.com/t/creating-custom-instrumentation-for-non-iis-net-applications/39436
    /// </summary>
    public class NewRelicCustomInstrumentationFilterAttribute : MagicOnionFilterAttribute
    {
        const string UnaryTransactionCategory = "MagicOnion-Unary";

        public NewRelicCustomInstrumentationFilterAttribute() : this(null) { }

        public NewRelicCustomInstrumentationFilterAttribute(Func<ServiceContext, Task> next) : base(next) { }

        public override Task Invoke(ServiceContext context)
        {
            if (context.MethodType == MethodType.Unary)
            {
                // Support only Unary.
                return InstrumentInvoke(context);
            }
            else
            {
                return Next.Invoke(context);
            }
        }

        public async Task InstrumentInvoke(ServiceContext context)
        {
            var method = context.CallContext.Method;
            NewRelic.SetTransactionName(UnaryTransactionCategory, method);
            try
            {
                await Next.Invoke(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                NewRelic.NoticeError(ex);
                throw;
            }
        }
    }

    internal static class NewRelic
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SetTransactionName(string category, string name)
        {
            Trace.WriteLine(string.Format("NewRelic.SetTransactionName({0},{1})", category, name));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void NoticeError(Exception exception)
        {
            Trace.WriteLine(string.Format("NewRelic.NoticeError({0})", exception));
        }
    }
}