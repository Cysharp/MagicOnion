using System;
using System.Security.Principal;
using MagicOnion.Server.Authentication;

namespace JwtAuthApp.Server.Authentication
{
    public class CustomJwtAuthUserIdentity : IIdentity
    {
        public long UserId { get; }

        public bool IsAuthenticated => true;
        public string AuthenticationType => "Jwt";

        public string Name { get; }

        public CustomJwtAuthUserIdentity(long userId, string displayName)
        {
            UserId = userId;
            Name = displayName ?? throw new ArgumentNullException(nameof(displayName));
        }
    }
}