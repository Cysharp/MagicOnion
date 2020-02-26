using System;
using System.Threading.Tasks;
using Grpc.Core;
using JwtAuthApp.Shared;
using MagicOnion.Client;

namespace JwtAuthApp.Client
{
    class Program : ITimerHubReceiver
    {
        static Task Main(string[] args)
        {
            return new Program().MainCore(args);
        }

        private async Task MainCore(string[] args)
        {
            var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);

            // 1. Call an API without an authentication token.
            var accountClient = MagicOnionClient.Create<IAccountService>(channel);
            var user = await accountClient.GetCurrentUserNameAsync();
            Console.WriteLine($@"[IAccountService.GetCurrentUserNameAsync] Current User: UserId={user.UserId}; IsAuthenticated={user.IsAuthenticated}; Name={user.Name}");
            try
            {
                var greeterClientAnon = MagicOnionClient.Create<IGreeterService>(channel);
                Console.WriteLine($"[IGreeterService.HelloAsync] {await greeterClientAnon.HelloAsync()}");
            }
            catch (RpcException e)
            {
                Console.WriteLine($"[IGreeterService.HelloAsync] Exception: {e.Message}");
            }

            // 2. Sign-in with ID and password and receive an authentication token.
            var signInId = "kyaru@example.com";
            var password = "P@ssword2";
            Console.WriteLine($@"[IAccountService.SignInAsync] Try signing in as '{signInId}'...");
            var authResult = await accountClient.SignInAsync(signInId, password);
            if (authResult.Success)
            {
                Console.WriteLine($@"[IAccountService.SignInAsync] User authenticated as {authResult.Name} (UserId:{authResult.UserId})");
            }
            else
            {
                throw new Exception("[IAccountService.SignInAsync] Authentication failed.");
            }

            // 3. Get the user information using the authentication token.
            accountClient = MagicOnionClient.Create<IAccountService>(channel)
                .WithHeaders(new Metadata
                {
                    { "auth-token-bin", authResult.Token }
                });
            user = await accountClient.GetCurrentUserNameAsync();
            Console.WriteLine($@"[IAccountService.GetCurrentUserNameAsync] Current User: UserId={user.UserId}; IsAuthenticated={user.IsAuthenticated}; Name={user.Name}");

            // 4. Call an API with the authentication token.
            var greeterClient = MagicOnionClient.Create<IGreeterService>(channel)
                .WithHeaders(new Metadata
                {
                    { "auth-token-bin", authResult.Token }
                });
            Console.WriteLine($"[IGreeterService.HelloAsync] {await greeterClient.HelloAsync()}");

            // 5. Call StreamingHub with authentication
            var timerHubClient = StreamingHubClient.Connect<ITimerHub, ITimerHubReceiver>(
                channel,
                this,
                option: new CallOptions().WithHeaders(new Metadata()
                {
                    { "auth-token-bin", authResult.Token }
                }));
            await timerHubClient.SetAsync(TimeSpan.FromSeconds(5));
            await Task.Yield(); // NOTE: Release the gRPC's worker thread here.

            // 6. Insufficient privilege (The current user is not in administrators role).
            try
            {
                await accountClient.DangerousOperationAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[IAccountService.DangerousOperationAsync] Exception: {e.Message}");
            }

            Console.ReadLine();
        }

        void ITimerHubReceiver.OnTick(string message)
        {
            Console.WriteLine($"[ITimerHubReceiver.OnTick] {message}");
        }
    }
}
