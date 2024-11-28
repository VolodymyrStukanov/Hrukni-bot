using Telegram.Bot;
using Microsoft.Extensions.Logging;
using HrukniHohlinaBot.Services.Interfaces;

namespace HrukniHohlinaBot.Services.BotServices
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly ITelegramBotClient botClient;
        private readonly ILogger<TelegramBotService> logger;
        private readonly IResetHoholService resetHoholService;
        private readonly IUpdateHandlerService updateHandlerService;

        public TelegramBotService(ITelegramBotClient botClient, ILogger<TelegramBotService> logger,
            IResetHoholService hoholService, IUpdateHandlerService updateHandlerService)
        {
            this.logger = logger;
            this.botClient = botClient;
            resetHoholService = hoholService;
            this.updateHandlerService = updateHandlerService;
        }

        public async Task StartBotAsync(CancellationToken cancellationToken)
        {
            new Thread(() => SetNewHohols()).Start();
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
                    Thread.Sleep(waitingTime["second"]);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occurred in StartBotAsync method");
                }
            }
        }


        private readonly TimeOnly resetHoholsTime = new TimeOnly(6, 0, 0);
        private readonly Dictionary<string, int> waitingTime = new Dictionary<string, int>()
        {
            { "second", 1000 },
            { "tenSeconds", 10000 },
            { "minute", 60000 },
            { "hour", 3600000 }
        };
        private void SetNewHohols()
        {
            try
            {
                while (true)
                {
                    var time = DateTime.Now;

                    if (time.Hour == resetHoholsTime.Hour
                        && time.Minute == resetHoholsTime.Minute)
                    {
                        resetHoholService.ResetHohols();
                        Thread.Sleep(waitingTime["hour"]);
                    }

                    if (time.Hour != resetHoholsTime.Hour - 1) 
                        Thread.Sleep(waitingTime["hour"]);
                    else if (time.Minute != (resetHoholsTime.Minute == 0 ? 60 - 1 : resetHoholsTime.Minute - 1)) 
                        Thread.Sleep(waitingTime["minute"]);
                    else 
                        Thread.Sleep(waitingTime["tenSeconds"]);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(exception: ex, $"An error occurred in SetNewHohols method");
            }
        }
    }
}
