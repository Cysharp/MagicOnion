using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
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
            var channel = GrpcChannel.ForAddress("http://localhost:5000");

            // 1. Call an API without an authentication token.
            {
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
            }

            // 3. Sign-in with ID and password and receive an authentication token. (WithAuthenticationFilter will acquire an authentication token automatically.)
            var signInId = "kyaru@example.com";
            var password = "P@ssword2";

            // 4. Get the user information using the authentication token.
            {
                var accountClient = MagicOnionClient.Create<IAccountService>(channel, new[] { new WithAuthenticationFilter(signInId, password, channel), });
                var user = await accountClient.GetCurrentUserNameAsync();
                Console.WriteLine($@"[IAccountService.GetCurrentUserNameAsync] Current User: UserId={user.UserId}; IsAuthenticated={user.IsAuthenticated}; Name={user.Name}");

                // 5. Call an API with the authentication token.
                var greeterClient = MagicOnionClient.Create<IGreeterService>(channel, new[] { new WithAuthenticationFilter(signInId, password, channel), });
                Console.WriteLine($"[IGreeterService.HelloAsync] {await greeterClient.HelloAsync()}");
            }

            // 5. Call StreamingHub with authentication
            {
                var timerHubClient = StreamingHubClient.Connect<ITimerHub, ITimerHubReceiver>(
                    channel,
                    this,
                    option: new CallOptions().WithHeaders(new Metadata()
                    {
                        { "auth-token-bin", AuthenticationTokenStorage.Current.Token }
                    }));
                await timerHubClient.SetAsync(TimeSpan.FromSeconds(5));
                await Task.Yield(); // NOTE: Release the gRPC's worker thread here.
            }

            // 6. Insufficient privilege (The current user is not in administrators role).
            {
                var accountClient = MagicOnionClient.Create<IAccountService>(channel, new[] { new WithAuthenticationFilter(signInId, password, channel), });
                try
                {
                    await accountClient.DangerousOperationAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[IAccountService.DangerousOperationAsync] Exception: {e.Message}");
                }
            }

            // 7. Refresh the token before calling an API.
            {
                await Task.Delay(1000 * 6); // The server is configured a token expiration set to 5 seconds.
                var greeterClient = MagicOnionClient.Create<IGreeterService>(channel, new[] { new WithAuthenticationFilter(signInId, password, channel), });
                Console.WriteLine($"[IGreeterService.HelloAsync] {await greeterClient.HelloAsync()}");
            }

            Console.ReadLine();
        }

        void ITimerHubReceiver.OnTick(string message)
        {
            Console.WriteLine($"[ITimerHubReceiver.OnTick] {message}");
        }
    }

    // NOTE: This implementation is for demonstration purpose only. DO NOT USE THIS IN PRODUCTION.
    class WithAuthenticationFilter : IClientFilter
    {
        private readonly string _signInId;
        private readonly string _password;
        private readonly GrpcChannel _channel;

        public WithAuthenticationFilter(string signInId, string password, GrpcChannel channel)
        {
            _signInId = signInId ?? throw new ArgumentNullException(nameof(signInId));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            if (AuthenticationTokenStorage.Current.IsExpired)
            {
                Console.WriteLine($@"[WithAuthenticationFilter/IAccountService.SignInAsync] Try signing in as '{_signInId}'... ({(AuthenticationTokenStorage.Current.Token == null ? "FirstTime" : "RefreshToken")})");

                var client = MagicOnionClient.Create<IAccountService>(_channel);
                var authResult = await client.SignInAsync(_signInId, _password);
                if (!authResult.Success)
                {
                    throw new Exception("Failed to sign-in on the server.");
                }
                Console.WriteLine($@"[WithAuthenticationFilter/IAccountService.SignInAsync] User authenticated as {authResult.Name} (UserId:{authResult.UserId})");

                AuthenticationTokenStorage.Current.Update(authResult.Token, authResult.Expiration); // NOTE: You can also read the token expiration date from JWT.

                context.CallOptions.Headers.Remove(new Metadata.Entry("auth-token-bin", Array.Empty<byte>()));
            }

            if (!context.CallOptions.Headers.Contains(new Metadata.Entry("auth-token-bin", Array.Empty<byte>())))
            {
                context.CallOptions.Headers.Add("auth-token-bin", AuthenticationTokenStorage.Current.Token);
            }

            return await next(context);
        }
    }

    // When the authentication filter acquires an authentication token, the token is stored in somewhere. (e.g. In-memory, JSON, PlayerPrefs, etc ...)
    // The token may be used repeatedly by multiple clients (MagicOnionClient or StreamingHubClient).
    class AuthenticationTokenStorage
    {
        public static AuthenticationTokenStorage Current { get; } = new AuthenticationTokenStorage();

        private readonly object _syncObject = new object();

        public byte[] Token { get; private set; }
        public DateTimeOffset Expiration { get; private set; }

        public bool IsExpired => Token == null || Expiration < DateTimeOffset.Now;

        public void Update(byte[] token, DateTimeOffset expiration)
        {
            lock (_syncObject)
            {
                Token = token ?? throw new ArgumentNullException(nameof(token));
                Expiration = expiration;
            }
        }
    }
}
