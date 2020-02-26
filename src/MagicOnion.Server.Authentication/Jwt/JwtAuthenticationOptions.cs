using System;
using System.Collections.Generic;
using System.Text;
using LitJWT;

namespace MagicOnion.Server.Authentication.Jwt
{
    public class JwtAuthenticationOptions
    {
        /// <summary>
        /// Gets or sets an JwtEncoder instance.
        /// </summary>
        public JwtEncoder Encoder { get; set; } = default!;

        /// <summary>
        /// Gets or sets an JwtDecoder instance.
        /// </summary>
        public JwtDecoder Decoder { get; set; } = default!;

        /// <summary>
        /// Gets or sets JWT expiration duration.
        /// </summary>
        public TimeSpan Expire { get; set; } = TimeSpan.FromDays(1);
    }
}
