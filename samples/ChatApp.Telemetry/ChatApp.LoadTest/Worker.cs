using ChatApp.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp.LoadTest
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly int clientAmount;
        private readonly Func<ChatHubClient> chatClientFactory;
        private readonly ConcurrentDictionary<Guid, ChatHubClient> _clients =
            new ConcurrentDictionary<Guid, ChatHubClient>();

        public Worker(Func<ChatHubClient> chatClientFactory, ILogger<Worker> logger, int clientAmount)
        {
            this.chatClientFactory = chatClientFactory;
            this.logger = logger;
            this.clientAmount = clientAmount;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Guid playerId;
            ChatHubClient chatClient;

            int i = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (i < clientAmount)
                {
                    playerId = Guid.NewGuid();
                    chatClient = chatClientFactory();

                    _clients[playerId] = chatClient;
                    await chatClient.JoinAsync(playerId);

                    i++;
                    //_logger.LogInformation("Clients added: {ClientAmount}", i);

                    if (i % 1000 == 0)
                        logger.LogInformation("Clients added: {ClientAmount}", i);
                }
                else
                    break;
            }
        }
    }
}
