using System.Security.Principal;
using LitJWT;

namespace MagicOnion.Server.Authentication.Jwt
{
    public interface IJwtAuthenticationProvider
    {
        DecodeResult TryCreatePrincipalFromToken(byte[] bytes, out IPrincipal principal);

        byte[] CreateTokenFromPayload(object payload);

        void ValidatePrincipal(ref JwtAuthenticationValidationContext ctx);
    }
}