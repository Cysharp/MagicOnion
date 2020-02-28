using System.Security.Principal;

namespace MagicOnion.Server.Authentication.Jwt
{
    public struct JwtAuthenticationValidationContext
    {
        public bool Rejected { get; private set; }
        public IPrincipal Principal { get; }

        public JwtAuthenticationValidationContext(IPrincipal principal)
        {
            Rejected = false;
            Principal = principal;
        }

        /// <summary>
        /// Rejects the current principal.
        /// </summary>
        public void Reject()
        {
            Rejected = true;
        }
    }
}