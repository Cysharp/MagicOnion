using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JwtAuthApp.Server.Authentication;
using JwtAuthApp.Shared;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Authentication;
using MagicOnion.Server.Authentication.Jwt;

namespace JwtAuthApp.Server.Services
{
    [Authorize]
    public class AccountService : ServiceBase<IAccountService>, IAccountService
    {
        private static IDictionary<string, (string Password, long UserId, string DisplayName)> DummyUsers = new Dictionary<string, (string, long, string)>(StringComparer.OrdinalIgnoreCase)
        {
            {"pecorine@example.com", ("P@ssw0rd1", 1001, "Eustiana von Astraea")},
            {"kyaru@example.com", ("P@ssword2", 1002, "Kiruya Momochi")},
        };

        private readonly IJwtAuthenticationProvider _jwtAuthProvider;

        public AccountService(IJwtAuthenticationProvider jwtAuthProvider)
        {
            _jwtAuthProvider = jwtAuthProvider ?? throw new ArgumentNullException(nameof(jwtAuthProvider));
        }

        [AllowAnonymous]
        public async UnaryResult<SignInResponse> SignInAsync(string signInId, string password)
        {
            await Task.Delay(1); // some workloads...

            if (DummyUsers.TryGetValue(signInId, out var userInfo) && userInfo.Password == password)
            {
                return new SignInResponse(
                    userInfo.UserId,
                    userInfo.DisplayName,
                    _jwtAuthProvider.CreateTokenFromPayload(new CustomJwtAuthenticationPayload() { UserId = userInfo.UserId, DisplayName = userInfo.DisplayName })
                );
            }

            return SignInResponse.Failed;
        }

        [AllowAnonymous]
        public async UnaryResult<CurrentUserResponse> GetCurrentUserNameAsync()
        {
            await Task.Delay(1); // some workloads...

            var identity = Context.GetPrincipal().Identity;
            if (identity is CustomJwtAuthUserIdentity customIdentity)
            {
                if (customIdentity.IsAuthenticated)
                {
                    var user = DummyUsers.SingleOrDefault(x => x.Value.UserId == customIdentity.UserId).Value;
                    return new CurrentUserResponse()
                    {
                        IsAuthenticated = true,
                        UserId = user.UserId,
                        Name = user.DisplayName,
                    };
                }
            }

            return CurrentUserResponse.Anonymous;
        }

        [Authorize(Roles = new[] {"Administrators"})]
        public async UnaryResult<string> DangerousOperationAsync()
        {
            await Task.Delay(1); // some workloads...

            return "rm -rf /";
        }
    }
}
