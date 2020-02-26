using System;
using System.Collections.Generic;
using System.Text;
using JwtAuthApp.Server.Authentication;
using JwtAuthApp.Shared;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Authentication;

namespace JwtAuthApp.Server.Services
{
    [Authorize]
    public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
    {
        public async UnaryResult<string> HelloAsync()
        {
            var identity = (CustomJwtAuthUserIdentity) Context.GetPrincipal().Identity;
            return $"Hello {identity.Name} (UserId:{identity.UserId})!";
        }
    }
}
