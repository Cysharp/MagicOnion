using System.Threading.Tasks;

namespace MagicOnion.Client
{
    internal static class InterceptInvokeHelper
    {
        public static ValueTask<ResponseContext> InvokeWithFilter(RequestContext context)
        {
            switch (context.Filters.Length)
            {
                case 0:
                    return new ValueTask<ResponseContext>(context.RequestMethod(context));
                case 1:
                    return InvokeWithFilter1(context);
                case 2:
                    return InvokeWithFilter2(context);
                case 3:
                    return InvokeWithFilter3(context);
                case 4:
                    return InvokeWithFilter4(context);
                case 5:
                    return InvokeWithFilter5(context);
                case 6:
                    return InvokeWithFilter6(context);
                case 7:
                    return InvokeWithFilter7(context);
                case 8:
                    return InvokeWithFilter8(context);
                case 9:
                    return InvokeWithFilter9(context);
                case 10:
                    return InvokeWithFilter10(context);
                default:
                    return InvokeRecursive(-1, context);
            }
        }

        static ValueTask<ResponseContext> InvokeWithFilter1(RequestContext context)
        {
            return context.Filters[0].SendAsync(context,
                ctx => new ValueTask<ResponseContext>(ctx.RequestMethod(ctx).WaitResponseAsync()));
        }

        static ValueTask<ResponseContext> InvokeWithFilter2(RequestContext context)
        {
            return context.Filters[0].SendAsync(context,
                x1 => x1.Filters[1].SendAsync(x1,
                    ctx => new ValueTask<ResponseContext>(ctx.RequestMethod(ctx).WaitResponseAsync())));
        }

        static ValueTask<ResponseContext> InvokeWithFilter3(RequestContext context)
        {
            return context.Filters[0].SendAsync(context,
                x1 => x1.Filters[1].SendAsync(x1,
                    x2 => x2.Filters[2].SendAsync(x2,
                        ctx => new ValueTask<ResponseContext>(ctx.RequestMethod(ctx).WaitResponseAsync()))));
        }

        static ValueTask<ResponseContext> InvokeWithFilter4(RequestContext context)
        {
            return context.Filters[0].SendAsync(context,
                x1 => x1.Filters[1].SendAsync(x1,
                    x2 => x2.Filters[2].SendAsync(x2,
                        x3 => x3.Filters[3].SendAsync(x3,
                            ctx => new ValueTask<ResponseContext>(ctx.RequestMethod(ctx).WaitResponseAsync())))));
        }

        static ValueTask<ResponseContext> InvokeWithFilter5(RequestContext context)
        {
            return context.Filters[0].SendAsync(context,
                x1 => x1.Filters[1].SendAsync(x1,
                    x2 => x2.Filters[2].SendAsync(x2,
                        x3 => x3.Filters[3].SendAsync(x3,
                            x4 => x4.Filters[4].SendAsync(x4,
                                ctx => new ValueTask<ResponseContext>(ctx.RequestMethod(ctx).WaitResponseAsync()))))));
        }

        static ValueTask<ResponseContext> InvokeWithFilter6(RequestContext context)
        {
            return context.Filters[0].SendAsync(context,
                x1 => x1.Filters[1].SendAsync(x1,
                    x2 => x2.Filters[2].SendAsync(x2,
                        x3 => x3.Filters[3].SendAsync(x3,
                            x4 => x4.Filters[4].SendAsync(x4,
                                x5 => x5.Filters[5].SendAsync(x5,
                                    ctx => new ValueTask<ResponseContext>(ctx.RequestMethod(ctx).WaitResponseAsync())))))));
        }

        static ValueTask<ResponseContext> InvokeWithFilter7(RequestContext context)
        {
            return context.Filters[0].SendAsync(context,
                x1 => x1.Filters[1].SendAsync(x1,
                    x2 => x2.Filters[2].SendAsync(x2,
                        x3 => x3.Filters[3].SendAsync(x3,
                            x4 => x4.Filters[4].SendAsync(x4,
                                x5 => x5.Filters[5].SendAsync(x5,
                                    x6 => x6.Filters[6].SendAsync(x6,
                                        ctx => new ValueTask<ResponseContext>(ctx.RequestMethod(ctx).WaitResponseAsync()))))))));
        }

        static ValueTask<ResponseContext> InvokeWithFilter8(RequestContext context)
        {
            return context.Filters[0].SendAsync(context,
                x1 => x1.Filters[1].SendAsync(x1,
                    x2 => x2.Filters[2].SendAsync(x2,
                        x3 => x3.Filters[3].SendAsync(x3,
                            x4 => x4.Filters[4].SendAsync(x4,
                                x5 => x5.Filters[5].SendAsync(x5,
                                    x6 => x6.Filters[6].SendAsync(x6,
                                        x7 => x7.Filters[7].SendAsync(x7,
                                            ctx => new ValueTask<ResponseContext>(ctx.RequestMethod(ctx).WaitResponseAsync())))))))));
        }

        static ValueTask<ResponseContext> InvokeWithFilter9(RequestContext context)
        {
            return context.Filters[0].SendAsync(context,
                x1 => x1.Filters[1].SendAsync(x1,
                    x2 => x2.Filters[2].SendAsync(x2,
                        x3 => x3.Filters[3].SendAsync(x3,
                            x4 => x4.Filters[4].SendAsync(x4,
                                x5 => x5.Filters[5].SendAsync(x5,
                                    x6 => x6.Filters[6].SendAsync(x6,
                                        x7 => x7.Filters[7].SendAsync(x7,
                                            x8 => x8.Filters[8].SendAsync(x8,
                                                ctx => new ValueTask<ResponseContext>(ctx.RequestMethod(ctx).WaitResponseAsync()))))))))));
        }

        static ValueTask<ResponseContext> InvokeWithFilter10(RequestContext context)
        {
            return context.Filters[0].SendAsync(context,
                x1 => x1.Filters[1].SendAsync(x1,
                    x2 => x2.Filters[2].SendAsync(x2,
                        x3 => x3.Filters[3].SendAsync(x3,
                            x4 => x4.Filters[4].SendAsync(x4,
                                x5 => x5.Filters[5].SendAsync(x5,
                                    x6 => x6.Filters[6].SendAsync(x6,
                                        x7 => x7.Filters[7].SendAsync(x7,
                                            x8 => x8.Filters[8].SendAsync(x8,
                                                x9 => x9.Filters[9].SendAsync(x9,
                                                    ctx => new ValueTask<ResponseContext>(ctx.RequestMethod(ctx).WaitResponseAsync())))))))))));
        }

        // for invoke N filters(slow path).

        static ValueTask<ResponseContext> InvokeRecursive(int index, RequestContext context)
        {
            index += 1; // start from -1
            if (index != context.Filters.Length)
            {
                return context.Filters[index].SendAsync(context, ctx => InvokeRecursive(index, ctx));
            }
            else
            {
                return new ValueTask<ResponseContext>(context.RequestMethod(context).WaitResponseAsync());
            }
        }
    }
}