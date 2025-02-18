﻿using Telegram.Bot;
using Microsoft.Extensions.Logging;
using HrukniBot.Services.BotServices;
using Microsoft.Extensions.Hosting;

namespace HrukniHohlinaBot.Services.BotServices
{
    public class TelegramBotService : BackgroundService
    {
        private readonly ITelegramBotClient botClient;
        private readonly ILogger<TelegramBotService> logger;
        private readonly IUpdateHandlerService updateHandlerService;

        public TelegramBotService(ITelegramBotClient botClient,
            ILogger<TelegramBotService> logger, 
            IUpdateHandlerService updateHandlerService)
        {
            this.logger = logger;
            this.botClient = botClient;
            this.updateHandlerService = updateHandlerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int offset = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var updates = await botClient.GetUpdates(offset, timeout: 100, cancellationToken: stoppingToken);
                    foreach (var update in updates)
                    {
                        await updateHandlerService.HandleUpdate(update);
                        offset = update.Id + 1;
                    }
                }
                catch(Telegram.Bot.Exceptions.RequestException ex)
                {
                    logger.LogError(ex, $"Problem with receiving updates. Possible problems with the internet connection");
                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occurred in StartBotAsync method");
                }
            }
        }
    }
}
