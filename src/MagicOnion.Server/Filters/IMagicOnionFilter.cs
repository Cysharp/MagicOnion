using System;
using System.Threading.Tasks;

namespace MagicOnion.Server.Filters;

public interface IMagicOnionFilter : IMagicOnionFilterMetadata
{
    ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next);
}