using HrukniHohlinaBot.DB;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using HrukniHohlinaBot.Services.ChatsServices;
using HrukniHohlinaBot.Services.HoholServices;
using HrukniHohlinaBot.Services.MemberServices;
using HrukniHohlinaBot.Services.SettingsServices;

namespace HrukniHohlinaBot.Bot
{
    public class TgBot
    {
        private TelegramBotClient _client;

        public TgBot() { }

        public void Start()
        {
            using var cts = new CancellationTokenSource();
            SettingsService settingsService = new SettingsService(GetApplicationDbContext());
            var setting = settingsService.GetSettingsById(-1);
            if(setting == null)
            {
                Console.WriteLine("ERROR. Wrong settings id");
                return;
            }

            new Thread(() => SetNewHohols()).Start();

            _client = new TelegramBotClient(setting.Token, cancellationToken: cts.Token);

            var updateHandler = new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync);
            _client.StartReceiving(updateHandler);

            Console.ReadLine();

            cts.Cancel();
        }

        private ApplicationDbContext GetApplicationDbContext()
        {
            string dbConnectString = "Username=postgres;Password=postgres;Host=localhost;Port=5432;Database=HrukniHohlinaBot;";
            var optionBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionBuilder.UseNpgsql(dbConnectString);

            return new ApplicationDbContext(optionBuilder.Options);
        }
        
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            ChatsService chatsService = new ChatsService(GetApplicationDbContext());
            HoholService hoholService = new HoholService(GetApplicationDbContext());
            MemberService memberService = new MemberService(GetApplicationDbContext());

            BotEventHandler handler = new BotEventHandler(_client, chatsService, hoholService, memberService);
            await handler.HandleUpdate(botClient, update, cancellationToken);
        }

        
        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            ChatsService chatsService = new ChatsService(GetApplicationDbContext());
            HoholService hoholService = new HoholService(GetApplicationDbContext());
            MemberService memberService = new MemberService(GetApplicationDbContext());

            BotEventHandler handler = new BotEventHandler(_client, chatsService, hoholService, memberService);
            handler.HandleError(botClient, exception, cancellationToken);

            return Task.CompletedTask;
        }

        private TimeOnly resetHoholsTime = new TimeOnly(6,0,0);
        private Dictionary<string, int> waitingTime = new Dictionary<string, int>()
        {
            { "second", 1000 },
            { "minute", 60000 },
            { "hour", 3600000 }
        };
        private void SetNewHohols()
        {
            HoholService hoholService = new HoholService(GetApplicationDbContext());

            while (true)
            {
                var time = DateTime.Now.ToUniversalTime();

                if (time.Hour == resetHoholsTime.Hour
                    && time.Minute == 0
                    && time.Second == 0)
                    hoholService.ResetHohols();

                if (time.Second > 0) Thread.Sleep(waitingTime["second"]);
                if (time.Minute > 0) Thread.Sleep(waitingTime["minute"]);
                else Thread.Sleep(waitingTime["hour"]);
            }
        }
    }
}
