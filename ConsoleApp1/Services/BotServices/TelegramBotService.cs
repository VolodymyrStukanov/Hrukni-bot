using Telegram.Bot;
using Microsoft.Extensions.Logging;
using HrukniHohlinaBot.Services.Interfaces;

namespace HrukniHohlinaBot.Services.BotServices
{
    public class TelegramBotService : ITelegramBotService
    {
        private ITelegramBotClient _botClient;
        private readonly ILogger<TelegramBotService> _logger;
        private IResetHoholService _hoholService;
        private IUpdateHandlerService _updateHandlerService;

        public TelegramBotService(ITelegramBotClient botClient, ILogger<TelegramBotService> logger,
            IResetHoholService hoholService, IUpdateHandlerService updateHandlerService)
        {
            _logger = logger;
            _botClient = botClient;
            _hoholService = hoholService;
            _updateHandlerService = updateHandlerService;
        }

        public async Task StartBotAsync(CancellationToken cancellationToken)
        {
            new Thread(() => SetNewHohols()).Start();
            int offset = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var updates = await _botClient.GetUpdatesAsync(offset, timeout: 100, cancellationToken: cancellationToken);
                    foreach (var update in updates)
                    {
                        await _updateHandlerService.HandleUpdate(update);
                        offset = update.Id + 1;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred in StartBotAsync method: {ex.Message}");
                }
            }
        }


        private TimeOnly resetHoholsTime = new TimeOnly(6, 0, 0);
        private Dictionary<string, int> waitingTime = new Dictionary<string, int>()
        {
            { "tenSeconds", 10000 },
            { "minute", 60000 },
            { "hour", 3600000 }
        };
        private void SetNewHohols()
        {
            while (true)
            {
                try
                {
                    var time = DateTime.Now;

                    if (time.Hour == resetHoholsTime.Hour
                        && time.Minute == resetHoholsTime.Minute)
                    {
                        _hoholService.ResetHohols();
                        Thread.Sleep(waitingTime["hour"]);
                    }

                    if (time.Hour != resetHoholsTime.Hour - 1) 
                        Thread.Sleep(waitingTime["hour"]);
                    else if (time.Minute != (resetHoholsTime.Minute == 0 ? 60 - 1 : resetHoholsTime.Minute - 1)) 
                        Thread.Sleep(waitingTime["minute"]);
                    else 
                        Thread.Sleep(waitingTime["tenSeconds"]);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred in SetNewHohols method: {ex.Message}");
                }
            }
        }
    }
}
