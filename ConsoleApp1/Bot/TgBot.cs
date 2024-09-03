using ConsoleApp1.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using ConsoleApp1.Services;
using ConsoleApp1.DB.Models;

namespace ConsoleApp1.Bot
{
    public class TgBot
    {
        private TelegramBotClient _client;
        private ChatsService _chatsService;
        private SettingsService _settingsService;
        private HoholService _hoholService;
        private MemberService _memberService;

        static string[] responseMessage = new string[]
        {
            "похрюкай",
            "похрюкай хохол",
            "хрюкай",
            "давай хрюкай",
            "скажи хрю хрю",
            "хрююююю",
        };

        public TgBot(ChatsService chatsService, SettingsService settingsService, HoholService hoholService, MemberService memberService)
        {
            _chatsService = chatsService;
            _settingsService = settingsService;
            _hoholService = hoholService;
            _memberService = memberService;
        }
        public void Start()
        {
            using var cts = new CancellationTokenSource();
            var setting = _settingsService.GetSettingsById(-1);
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

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //if (update.Message.Text is null) return;
            //else
            //{
                if (update.Message.Text == "/start")
                {
                    _chatsService.AddChat(new DB.Models.Chat()
                    {
                        Id = update.Message.Chat.Id,
                    });

                    await _client.SendTextMessageAsync(update.Message.Chat.Id, $"Хохлы...");
                    var hohol = _hoholService.GetActiveHoholByChatId(update.Message.Chat.Id);
                    if (hohol != null)
                    {
                        _hoholService.ResetHoholForChat(update.Message.Chat.Id);
                        await _client.SendTextMessageAsync(update.Message.Chat.Id, $"Готовтесь хрюкать");
                    }
                }
                else
                {
                    var chat = _chatsService.GetChat(update.Message.Chat.Id);
                    if (chat == null) return;

                    var message = update.Message;
                    var memberInfo = await _client.GetChatMemberAsync(message.Chat.Id, message.From.Id);
                    AddNewMember(message.Chat.Id, memberInfo.User.Username);

                    if (update.Type == UpdateType.Message)
                    {
                        var hohol = new Hohol()
                        {
                            Username = memberInfo.User.Username,
                            ChatId = message.Chat.Id,
                        };
                        if (_hoholService.IsActiveHohol(hohol))
                        {
                            if (!hohol.IsAllowedToWrite())
                            {
                                await _client.DeleteMessageAsync(
                                    chatId: hohol.ChatId,
                                    messageId: update.Message.MessageId
                                );

                                Random rand = new Random();
                                int i = rand.Next(0, responseMessage.Length);

                                await _client.SendTextMessageAsync(
                                    chatId: hohol.ChatId,
                                    text: $"@{hohol.Username} {responseMessage[i]}",
                                    cancellationToken: cancellationToken
                                );
                            }
                        }
                    }
                }
            //}
        }

        private void AddNewMember(long chatId, string username)
        {
            if(_memberService.GetMember(username, chatId) != null) return;
            Member member = new Member()
            {
                ChatId = chatId,
                Username = username,
            };
            _memberService.AddMember(member);
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"An error occurred: {exception.Message}");
            return Task.CompletedTask;
        }

        //private Dictionary<string, Action<Update>> CommandHandlers = new Dictionary<string, Action<Update>>()
        //{
        //    { "/start", HandleStart }
        //};

        //private void HandleStart(Update update)
        //{

        //}


        private TimeOnly updateHoholTime = new TimeOnly(6, 0, 0);
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
                var time = DateTime.Now;

                if (time.Hour == updateHoholTime.Hour)
                {
                    _hoholService.ResetHohols();
                }

                if (time.Second > 0) Thread.Sleep(waitingTime["second"]);
                if (time.Minute > 0) Thread.Sleep(waitingTime["minute"]);
                else Thread.Sleep(waitingTime["hour"]);
            }
        }
    }
}
