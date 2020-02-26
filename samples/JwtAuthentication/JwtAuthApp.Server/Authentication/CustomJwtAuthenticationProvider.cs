using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using LitJWT;
using MagicOnion.Server.Authentication.Jwt;
using Microsoft.Extensions.Options;

namespace JwtAuthApp.Server.Authentication
{
    public class CustomJwtAuthenticationProvider : IJwtAuthenticationProvider
    {
        private readonly JwtAuthenticationOptions _jwtAuthOptions;

        public CustomJwtAuthenticationProvider(IOptions<JwtAuthenticationOptions> jwtAuthOptions)
        {
            _jwtAuthOptions = jwtAuthOptions.Value;
        }

        public DecodeResult TryCreatePrincipalFromToken(byte[] bytes, out IPrincipal principal)
        {
            var result = _jwtAuthOptions.Decoder.TryDecode(bytes, x => JsonSerializer.Deserialize<CustomJwtAuthenticationPayload>(x), out var payload);
            if (result != DecodeResult.Success)
            {
                principal = null;
                return result;
            }

            principal = new GenericPrincipal(new CustomJwtAuthUserIdentity(payload.UserId, payload.DisplayName), Array.Empty<string>());
            return DecodeResult.Success;
        }

        public byte[] CreateTokenFromPayload(object payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            return _jwtAuthOptions.Encoder.EncodeAsUtf8Bytes(
                payload,
                DateTimeOffset.Now.Add(_jwtAuthOptions.Expiry),
                (o, writer) => writer.Write(JsonSerializer.SerializeToUtf8Bytes(o)));
        }

        public void ValidatePrincipal(ref JwtAuthenticationValidationContext ctx)
        {
        }
    }
}
