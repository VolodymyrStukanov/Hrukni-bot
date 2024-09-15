using Telegram.Bot;
using HrukniHohlinaBot.Services.HoholServices;

namespace HrukniHohlinaBot.Services.BotServices
{
    public class TelegramBotService
    {
        private ITelegramBotClient _botClient;
        private BotEventHandler _botEventHandler;
        private HoholService _hoholService;

        public TelegramBotService(ITelegramBotClient botClient, BotEventHandler botEventHandler,
            HoholService hoholService)
        {
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
                    await _botEventHandler.HandleError(ex);
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
                    _botEventHandler.HandleError(ex);
                }
            }
        }
    }
}
