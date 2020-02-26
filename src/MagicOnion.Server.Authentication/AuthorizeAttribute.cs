using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace MagicOnion.Server.Authentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeAttribute : Attribute, IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        private class AuthorizeFilter : MagicOnionFilterAttribute
        {
            private readonly string[] _roles;

            public AuthorizeFilter(string[] roles)
            {
                _roles = roles;
            }

            public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
            {
                if (context.AttributeLookup.Contains(typeof(AllowAnonymousAttribute)))
                {
                    return next(context);
                }

                var principal = context.GetPrincipal();
                if (principal == null)
                {
                    throw new ReturnStatusException(StatusCode.Unauthenticated, "Authentication is not configured on the server.");
                }

                if (principal.Identity == null || !principal.Identity.IsAuthenticated)
                {
                    throw new ReturnStatusException(StatusCode.Unauthenticated, "The user is not authenticated.");
                }

                if (_roles != null && _roles.Length != 0)
                {
                    if (!_roles.Any(x => principal.IsInRole(x)))
                    {
                        throw new ReturnStatusException(StatusCode.PermissionDenied, "The user is not authorized.");
                    }
                }

                return next(context);
            }
        }

        public MagicOnionFilterAttribute CreateInstance(IServiceLocator serviceLocator)
        {
            return new AuthorizeFilter(Roles);
        }

        public int Order { get; set; }

        public string[] Roles { get; set; }
    }
}
