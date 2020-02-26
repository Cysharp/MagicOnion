using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using MagicOnion.Server;
using MagicOnion.Server.Authentication;

// ReSharper disable once CheckNamespace

namespace MagicOnion.Server
{
    public static class ServiceContextAuthenticationExtensions
    {
        internal static void SetPrincipal(this ServiceContext context, IPrincipal principal)
        {
            context.Items[MagicOnionPrincipal.ServiceContextItemKeyPrincipal] = principal;
        }

        public static IPrincipal GetPrincipal(this ServiceContext context)
        {
            return context.Items[MagicOnionPrincipal.ServiceContextItemKeyPrincipal] as IPrincipal ?? MagicOnionPrincipal.AnonymousPrincipal;
        }
    }
}
