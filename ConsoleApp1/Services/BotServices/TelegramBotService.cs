using Telegram.Bot;
using HrukniHohlinaBot.Services.HoholServices;
using Microsoft.Extensions.Logging;

namespace HrukniHohlinaBot.Services.BotServices
{
    public class TelegramBotService
    {
        private ITelegramBotClient _botClient;
        private readonly ILogger<TelegramBotService> _logger;
        private BotEventHandler _botEventHandler;
        private HoholService _hoholService;

        public TelegramBotService(ITelegramBotClient botClient, ILogger<TelegramBotService> logger,
            BotEventHandler botEventHandler, HoholService hoholService)
        {
            _logger = logger;
            _botClient = botClient;
            _botEventHandler = botEventHandler;
            _hoholService = hoholService;
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
                        await _botEventHandler.HandleUpdate(update);
                        offset = update.Id + 1;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred: {ex.Message}");
                }
            }
        }


        private TimeOnly resetHoholsTime = new TimeOnly(6, 0, 0);
        private Dictionary<string, int> waitingTime = new Dictionary<string, int>()
        {
            { "second", 1000 },
            { "minute", 60000 },
            { "hour", 3600000 }
        };
        private void SetNewHohols()
        {
            while (true)
            {
                try
                {
                    var time = DateTime.Now.ToUniversalTime();

                    if (time.Hour == resetHoholsTime.Hour
                        && time.Minute == 0
                        && time.Second == 0)
                        _hoholService.ResetHohols();

                    if (time.Second > 0) Thread.Sleep(waitingTime["second"]);
                    if (time.Minute > 0) Thread.Sleep(waitingTime["minute"]);
                    else Thread.Sleep(waitingTime["hour"]);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}
