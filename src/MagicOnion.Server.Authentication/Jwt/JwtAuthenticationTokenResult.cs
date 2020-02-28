using System;

namespace MagicOnion.Server.Authentication.Jwt
{
    /// <summary>
    /// The struct contains JWT encoded token and its expiration date.
    /// </summary>
    public readonly struct JwtAuthenticationTokenResult
    {
        /// <summary>
        /// Gets the JWT encoded token.
        /// </summary>
        public byte[] Token { get; }

        /// <summary>
        /// Gets an expiration date of the JWT token.
        /// </summary>
        public DateTimeOffset Expiration { get; }

        public JwtAuthenticationTokenResult(byte[] token, DateTimeOffset expiration)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            Expiration = expiration;
        }
    }
}