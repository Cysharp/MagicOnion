using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core;
using LitJWT;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Authentication.Jwt
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class JwtAuthenticationAttribute : Attribute, IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        private class JwtAuthenticationFilter : MagicOnionFilterAttribute
        {
            private readonly IJwtAuthenticationProvider _jwtAuthProvider;
            private readonly bool _isAuthTokenRequired;
            private readonly string _requestHeaderKey;

            public JwtAuthenticationFilter(IJwtAuthenticationProvider jwtAuthProvider, string requestHeaderKey, bool isAuthTokenRequired)
            {
                _jwtAuthProvider = jwtAuthProvider ?? throw new ArgumentNullException(nameof(jwtAuthProvider));
                _requestHeaderKey = requestHeaderKey ?? throw new ArgumentNullException(nameof(requestHeaderKey));
                _isAuthTokenRequired = isAuthTokenRequired;
            }

            public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
            {
                var metadataAuthToken = context.CallContext.RequestHeaders.Get(_requestHeaderKey);
                if (metadataAuthToken == null)
                {
                    if (_isAuthTokenRequired)
                    {
                        throw new ReturnStatusException(StatusCode.Unauthenticated, "An authentication token is missing.");
                    }
                    else
                    {
                        context.SetPrincipal(MagicOnionPrincipal.AnonymousPrincipal);
                        return next(context);
                    }
                }

                if (!metadataAuthToken.IsBinary)
                {
                    throw new ReturnStatusException(StatusCode.Unauthenticated, "The authentication token is not binary type.");
                }

                var result = _jwtAuthProvider.TryCreatePrincipalFromToken(metadataAuthToken.ValueBytes, out var principal);
                if (result != DecodeResult.Success)
                {
                    throw new ReturnStatusException(StatusCode.Unauthenticated, $"The authentication token is invalid. (Reason: {result})");
                }
                if (principal == null)
                {
                    throw new ReturnStatusException(StatusCode.Unauthenticated, $"The authentication token is invalid. (Reason: Principal is not provided)");
                }

                var validationCtx = new JwtAuthenticationValidationContext(principal);
                _jwtAuthProvider.ValidatePrincipal(ref validationCtx);
                if (validationCtx.Rejected)
                {
                    throw new ReturnStatusException(StatusCode.Unauthenticated, $"The authentication token is invalid. (Reason: Rejected)");
                }

                context.SetPrincipal(principal);

                return next(context);
            }
        }

        public MagicOnionFilterAttribute CreateInstance(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetService<IOptions<JwtAuthenticationOptions>>().Value;
            var provider = serviceProvider.GetService<IJwtAuthenticationProvider>();
            return new JwtAuthenticationFilter(provider, options.RequestHeaderKey, options.IsAuthTokenRequired);
        }

        public int Order { get; set; }
    }
}
