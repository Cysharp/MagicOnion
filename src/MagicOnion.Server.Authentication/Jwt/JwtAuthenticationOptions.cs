using System;
using System.Collections.Generic;
using System.Text;
using LitJWT;

namespace MagicOnion.Server.Authentication.Jwt
{
    public class JwtAuthenticationOptions
    {
        public JwtEncoder Encoder { get; set; }
        public JwtDecoder Decoder { get; set; }
        public TimeSpan Expiry { get; set; } = TimeSpan.FromDays(1);
    }
}
