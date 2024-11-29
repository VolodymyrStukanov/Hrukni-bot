using Telegram.Bot;
using Microsoft.Extensions.Logging;
using HrukniBot.Services.BotServices;

namespace HrukniHohlinaBot.Services.BotServices
{
    public class TelegramBotService : ITelegramBotService
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

        public async Task StartBotAsync(CancellationToken cancellationToken)
        {
            int offset = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var updates = await botClient.GetUpdatesAsync(offset, timeout: 100, cancellationToken: cancellationToken);
                    foreach (var update in updates)
                    {
                        await updateHandlerService.HandleUpdate(update);
                        offset = update.Id + 1;
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occurred in StartBotAsync method");
                }
            }
        }
    }
}
