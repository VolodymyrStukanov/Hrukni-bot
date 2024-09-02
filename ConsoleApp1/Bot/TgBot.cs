using ConsoleApp1.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace ConsoleApp1.Bot
{
    public class TgBot
    {
        private TelegramBotClient _client;
        private ApplicationDbContext _context;

        public TgBot(ApplicationDbContext context)
        {
            _context = context;
        }
        public void Start()
        {
            _client = new TelegramBotClient(_context.Settings.FirstOrDefault(x => x.Id == -1).Token);
            using var cts = new CancellationTokenSource();


            //??????????????????????????????????????????????????
            //var receiverOptions = new ReceiverOptions
            //{
            //    AllowedUpdates = { }
            //};
            //_client.StartReceiving(
            //    HandleUpdateAsync,
            //    HandleErrorAsync,
            //    receiverOptions,
            //    cancellationToken: cts.Token);

            //var me = _client.GetMeAsync().Result;
            //Console.ReadLine();

            //cts.Cancel();
            //??????????????????????????????????????????????????

        }

    }
}
