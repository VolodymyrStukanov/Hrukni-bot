
using ConsoleApp1.Bot;
using ConsoleApp1.DB;
using ConsoleApp1.Services;
using Microsoft.EntityFrameworkCore;
//using Telegram.Bot;
//using Bot = ConsoleApp1.Bot.Bot;


namespace ConsoleApp1
{
    static class Program
    {
        static string dbConnectString = "Username=postgres;Password=postgres;Host=localhost;Port=5432;Database=HrukniHohlinaBot;";

        static void Main()
        {
            new Thread(RunBot).Start();
        }

        static void RunBot()
        {

            var optionBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionBuilder.UseNpgsql(dbConnectString);

            var context = new ApplicationDbContext(optionBuilder.Options);

            var chatService = new ChatsService(context);
            var settingsService = new SettingsService(context);
            var hoholService = new HoholService(context);
            var memberService = new MemberService(context);

            var Bot = new TgBot(chatService, settingsService, hoholService, memberService);
            Bot.Start();
            
        }
    }
}