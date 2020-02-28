using System.Security.Principal;
using LitJWT;

namespace MagicOnion.Server.Authentication.Jwt
{
    /// <summary>
    /// A provider serialize and deserialize JWT payload, and provides an authenticated principal.
    /// </summary>
    public interface IJwtAuthenticationProvider
    {
        /// <summary>
        /// Creates an authenticated user principal from JWT payload.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        DecodeResult TryCreatePrincipalFromToken(byte[] bytes, out IPrincipal? principal);

        /// <summary>
        /// Creates a byte-serialized payload from a payload object.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        JwtAuthenticationTokenResult CreateTokenFromPayload(object payload);

        /// <summary>
        /// Validates a principal. The methods will be called on executing the authentication process.
        /// </summary>
        /// <param name="ctx"></param>
        void ValidatePrincipal(ref JwtAuthenticationValidationContext ctx);
    }
}