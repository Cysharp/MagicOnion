using System;
using System.Collections.Generic;
using System.Text;
using LitJWT;

namespace MagicOnion.Server.Authentication.Jwt
{
    public class JwtAuthenticationOptions
    {
        /// <summary>
        /// Gets or sets a JwtEncoder instance.
        /// </summary>
        public JwtEncoder Encoder { get; set; } = default!;

        /// <summary>
        /// Gets or sets a JwtDecoder instance.
        /// </summary>
        public JwtDecoder Decoder { get; set; } = default!;

        /// <summary>
        /// Gets or sets JWT expiration duration.
        /// </summary>
        public TimeSpan Expire { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// Gets or sets a request header for an authentication token. the key must ends with "-bin". The default value is <value>auth-token-bin</value>.
        /// </summary>
        public string RequestHeaderKey { get; set; } = "auth-token-bin";

        /// <summary>
        /// Gets or sets whether an authentication token must be required or not. The default value is <value>false</value>.
        /// </summary>
        public bool IsAuthTokenRequired { get; set; } = false;
    }
}
