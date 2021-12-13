using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Grpc.Core;
using JwtAuthApp.Shared;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.AspNetCore.Authorization;

#pragma warning disable CS1998

namespace JwtAuthApp.Server.Services
{
    [Authorize]
    public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
    {
        public async UnaryResult<string> HelloAsync()
        {
            var userPrincipal = Context.CallContext.GetHttpContext().User;
            return $"Hello {userPrincipal.Identity?.Name} (UserId:{userPrincipal.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value})!";
        }
    }
}
