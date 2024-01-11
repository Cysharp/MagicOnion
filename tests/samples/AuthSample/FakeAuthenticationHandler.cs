using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AuthSample;

public class FakeAuthenticationHandler : AuthenticationHandler<FakeAuthenticationHandlerOptions>
{
#pragma warning disable CS0618 // Type or member is obsolete
    public FakeAuthenticationHandler(IOptionsMonitor<FakeAuthenticationHandlerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
#pragma warning restore CS0618 // Type or member is obsolete
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Context.Request.Headers.TryGetValue("Authorization", out var value) && value == "Bearer Alice")
        {
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new GenericIdentity("Alice")), "Fake")));
        }

        return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
    }
}

public class FakeAuthenticationHandlerOptions : AuthenticationSchemeOptions { }
