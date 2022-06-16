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

        public async Task<ServerStreamingResult<string>> ServerAsync(string name, int age)
        {
            var ctx = GetServerStreamingContext<string>();

            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                await ctx.WriteAsync($"{name} ({age}) @ {DateTime.Now}");
            }

            return ctx.Result();
        }

        public async Task<ClientStreamingResult<int, string>> ClientAsync()
        {
            var ctx = GetClientStreamingContext<int, string>();

            var items = new List<int>();
            await foreach (var item in ctx.ReadAllAsync())
            {
                items.Add(item);
            }

            return ctx.Result($"Received : {string.Join(",", items)} @ {DateTime.Now}");
        }

        public async Task<DuplexStreamingResult<int, string>> DuplexAsync()
        {
            var ctx = GetDuplexStreamingContext<int, string>();

            await foreach (var item in ctx.ReadAllAsync())
            {
                await ctx.WriteAsync($"Hello from Server @ {item}");
            }

            return ctx.Result();
        }
    }
}
