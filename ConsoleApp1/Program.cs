
using ConsoleApp1.Bot;


namespace ConsoleApp1
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