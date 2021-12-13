using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using JwtAuthApp.Server.Authentication;
using JwtAuthApp.Shared;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.AspNetCore.Authorization;

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

        private readonly JwtTokenService _jwtTokenService;

        public AccountService(JwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        }

        [AllowAnonymous]
        public async UnaryResult<SignInResponse> SignInAsync(string signInId, string password)
        {
            await Task.Delay(1); // some workloads...

            if (DummyUsers.TryGetValue(signInId, out var userInfo) && userInfo.Password == password)
            {
                var (token, expires) = _jwtTokenService.CreateToken(userInfo.UserId, userInfo.DisplayName);

                return new SignInResponse(
                    userInfo.UserId,
                    userInfo.DisplayName,
                    token,
                    expires
                );
            }

            return SignInResponse.Failed;
        }

        [AllowAnonymous]
        public async UnaryResult<CurrentUserResponse> GetCurrentUserNameAsync()
        {
            await Task.Delay(1); // some workloads...

            var userPrincipal = Context.CallContext.GetHttpContext().User;
            if (userPrincipal.Identity?.IsAuthenticated ?? false)
            {
                if (!int.TryParse(userPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value, out var userId))
                {
                    return CurrentUserResponse.Anonymous;
                }

                var user = DummyUsers.SingleOrDefault(x => x.Value.UserId == userId).Value;
                return new CurrentUserResponse()
                {
                    IsAuthenticated = true,
                    UserId = user.UserId,
                    Name = user.DisplayName,
                };
            }

            return CurrentUserResponse.Anonymous;
        }

        [Authorize(Roles = "Administrators")]
        public async UnaryResult<string> DangerousOperationAsync()
        {
            await Task.Delay(1); // some workloads...

            return "rm -rf /";
        }
    }
}
