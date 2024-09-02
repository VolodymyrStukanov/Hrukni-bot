
using ConsoleApp1.Bot;
using ConsoleApp1.DB;
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
            new Thread(delegate () { RunBot(); }).Start();
        }

        static void RunBot()
        {

            var optionBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionBuilder.UseNpgsql(dbConnectString);

            var context = new ApplicationDbContext(optionBuilder.Options);

            var Bot = new TgBot(context);
            
        }
    }
}