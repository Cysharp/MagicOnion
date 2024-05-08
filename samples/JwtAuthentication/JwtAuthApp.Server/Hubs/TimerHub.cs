using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using JwtAuthApp.Shared;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Multicaster;

namespace JwtAuthApp.Server.Hubs
{
    [Authorize]
    public class TimerHub : StreamingHubBase<ITimerHub, ITimerHubReceiver>, ITimerHub
    {
        private Task _timerLoopTask;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private TimeSpan _interval = TimeSpan.FromSeconds(1);
        private IMulticastGroup<ITimerHubReceiver> _group;

        public async Task SetAsync(TimeSpan interval)
        {
            if (_timerLoopTask != null) throw new InvalidOperationException("The timer has been already started.");

            _group = await this.Group.AddAsync(ConnectionId.ToString());
            _interval = interval;
            _timerLoopTask = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(_interval, _cancellationTokenSource.Token);

                    var userPrincipal = Context.CallContext.GetHttpContext().User;
                    Client.OnTick($"UserId={userPrincipal.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value}; Name={userPrincipal.Identity?.Name}");
                }
            });
        }

        protected override ValueTask OnDisconnected()
        {
            _cancellationTokenSource.Cancel();
            return base.OnDisconnected();
        }
    }
}
