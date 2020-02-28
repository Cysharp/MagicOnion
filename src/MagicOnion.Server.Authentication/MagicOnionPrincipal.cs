using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace MagicOnion.Server.Authentication
{
    public static class MagicOnionPrincipal
    {
        public const string ServiceContextItemKeyPrincipal = ".Principal";

        public static IPrincipal AnonymousPrincipal { get; } = new GenericPrincipal(UnauthenticatedIdentity.Instance, Array.Empty<string>());

        public static IIdentity AnonymousIdentity => UnauthenticatedIdentity.Instance;

        internal class UnauthenticatedIdentity : IIdentity
        {
            public static IIdentity Instance { get; } = new UnauthenticatedIdentity();

            public bool IsAuthenticated => false;
            public string AuthenticationType => "Unauthenticated";
            public string Name => "Anonymous";
        }
    }
}
