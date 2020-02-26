using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core;
using LitJWT;

namespace MagicOnion.Server.Authentication.Jwt
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class JwtAuthenticationAttribute : Attribute, IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        private class JwtAuthenticationFilter : MagicOnionFilterAttribute
        {
            private readonly IJwtAuthenticationProvider _jwtAuthProvider;
            private readonly bool _isAuthTokenRequired;
            private const string RequestHeaderKeyAuthTokenBin = "auth-token-bin";

            public JwtAuthenticationFilter(IJwtAuthenticationProvider jwtAuthProvider, bool isAuthTokenRequired = false)
            {
                _jwtAuthProvider = jwtAuthProvider ?? throw new ArgumentNullException(nameof(jwtAuthProvider));
                _isAuthTokenRequired = isAuthTokenRequired;
            }

            public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
            {
                var metadataAuthToken = context.CallContext.RequestHeaders.Get(RequestHeaderKeyAuthTokenBin);
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

        public MagicOnionFilterAttribute CreateInstance(IServiceLocator serviceLocator)
        {
            return new JwtAuthenticationFilter(serviceLocator.GetService<IJwtAuthenticationProvider>());
        }

        public int Order { get; set; }
    }
}
