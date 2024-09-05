
using HrukniHohlinaBot.Bot;


namespace HrukniHohlinaBot
{
    static class Program
    {
        static void Main()
        {
            new Thread(RunBot).Start();
        }

        static void RunBot()
        {
            var Bot = new TgBot();
            Bot.Start();            
        }
    }
}