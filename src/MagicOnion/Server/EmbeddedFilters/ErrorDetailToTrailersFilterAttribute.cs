using MessagePack;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion.Server.EmbeddedFilters
{
    // This filter is workaround of https://github.com/grpc/grpc/issues/9235
    // for communicate server to Unity
    public class ErrorDetailToTrailersFilterAttribute : MagicOnion.Server.MagicOnionFilterAttribute
    {
        const string ExceptionDetailKey = "exception_detail-bin";

        public ErrorDetailToTrailersFilterAttribute()
            : base(null)
        {
            this.Order = int.MinValue;
        }

        public ErrorDetailToTrailersFilterAttribute(Func<ServiceContext, Task> next)
            : base(next)
        {
            this.Order = int.MinValue;
        }

        public override async Task Invoke(ServiceContext context)
        {
            try
            {
                await Next.Invoke(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var msg = ex.ToString();
                var lineSplit = msg.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var sb = new StringBuilder();
                for (int i = 0; i < lineSplit.Length; i++)
                {

                    if (!(lineSplit[i].Contains("System.Runtime.CompilerServices")
                       || lineSplit[i].Contains("直前に例外がスローされた場所からのスタック トレースの終わり")
                       || lineSplit[i].Contains("End of stack trace from the previous location where the exception was thrown")
                       ))
                    {
                        sb.AppendLine(lineSplit[i]);
                    }
                    if (sb.Length >= 10000)
                    {
                        sb.AppendLine("----Omit Message(message size is too long)----");
                        break;
                    }
                }
                var str = sb.ToString();

                var bytes = LZ4MessagePackSerializer.Serialize(str);
                context.CallContext.ResponseTrailers.Add(ExceptionDetailKey, bytes);

                throw;
            }
        }
    }
}