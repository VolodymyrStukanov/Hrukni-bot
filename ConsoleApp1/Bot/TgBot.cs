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
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Bot
{
    public class TgBot
    {
        private TelegramBotClient _client;
        private ChatsService _chatsService;
        private SettingsService _settingsService;
        private HoholService _hoholService;
        private MemberService _memberService;

        static string[] claimMessages = new string[]
        {
            "похрюкай",
            "похрюкай хохол",
            "хохолина хрюкай",
            "хрюкай",
            "давай хрюкай",
            "скажи хрю хрю",
            "хрююююю",
        };

        static string[] allowationMessages = new string[]
        {
            @"Хороший хохол. Можеш хрюкать несколько часов, до {0}",
            @"Умничка. Хрюкаешь до {0}",
            @"Молодец, можеш розслабиться до {0}",
            @"До {0} можеш на совей свинячей балакать"
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

        private async Task<Member?> GetMember(Update update)
        {
            long chatId;
            long memberId;
            if (update.Type == UpdateType.Message && update.Message != null)
            {
                Message? message = update.Message;
                chatId = message.Chat.Id;
                memberId = message.From.Id;
            }
            else if (update.Type == UpdateType.MyChatMember && update.MyChatMember != null)
            {
                ChatMemberUpdated? myChatMember = update.MyChatMember;
                chatId = myChatMember.Chat.Id;
                memberId = myChatMember.From.Id;
            }
            else return null;

            ChatMember? memberInfo = await _client.GetChatMemberAsync(chatId, memberId);
            Member member = new Member()
            {
                Username = memberInfo.User.Username,
                ChatId = chatId,
                Id = memberId,
                IsOwner = memberInfo.Status == ChatMemberStatus.Creator
            };

            return member;
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Member? member = await GetMember(update);
            if (member == null) return;

            if (update.Type == UpdateType.Message 
                && update.Message != null 
                && update.Message.Text != null 
                && update.Message.Text[0] == '/')
            {
                await HandleCommand(member, update.Message);
                return;
            }

            if (_chatsService.GetChat(member.ChatId) == null) return;
            _memberService.AddMember(member);


            if (update.Type == UpdateType.Message && update.Message != null && update.Message.Text == null)
            {
                await ChatMemberUpdate(update.Message);
            }
            else if (update.Type == UpdateType.Message && update.Message != null && update.Message.Text != null)
            {
                await MessageUpdate(update.Message, member);
            }
            else if (update.Type == UpdateType.MyChatMember && update.MyChatMember != null)
            {
                await MyChatMemberUpdate(update.MyChatMember, member);
            }
        }

        private async Task HandleCommand(Member member, Message message)
        {
            if (message.Text[0] != '/') return;
            if (message.Text == "/start_hrukni" && member.IsOwner)
            {
                _chatsService.AddChat(new DB.Models.Chat()
                {
                    Id = message.Chat.Id,
                });

                await _client.SendTextMessageAsync(message.Chat.Id, $"Хохлы...");
                var hohol = _hoholService.GetActiveHoholByChatId(message.Chat.Id);
                if (hohol == null)
                {
                    _hoholService.ResetHoholForChat(message.Chat.Id);
                    await _client.SendTextMessageAsync(message.Chat.Id, $"Готовтесь хрюкать");
                }
            }
            else if(message.Text == "/stop_hrukni" && member.IsOwner)
            {
                _chatsService.RemoveChatById(message.Chat.Id);
            }
            else if(message.Text == "/reset_hohols" && member.IsOwner)
            {
                _hoholService.ResetHoholForChat(message.Chat.Id);
            }
        }

        private async Task ChatMemberUpdate(Message message)
        {
            if (message.NewChatMembers != null)
            {
                foreach (var newUser in message.NewChatMembers)
                {
                    if (newUser.IsBot) continue;

                    ChatMember? memberInfo = await _client.GetChatMemberAsync(message.Chat.Id, newUser.Id);
                    Member newMember = new Member()
                    {
                        Username = memberInfo.User.Username,
                        ChatId = message.Chat.Id,
                        Id = newUser.Id,
                        IsOwner = memberInfo.Status == ChatMemberStatus.Creator
                    };
                    _memberService.AddMember(newMember);
                }
            }
            else if (message.LeftChatMember != null)
            {
                _memberService.RemoveMember(message.LeftChatMember.Id, message.Chat.Id);
            }
        }

        private async Task MyChatMemberUpdate(ChatMemberUpdated myChatMember, Member member)
        {
            if(myChatMember.NewChatMember.Status == ChatMemberStatus.Left ||
            myChatMember.NewChatMember.Status == ChatMemberStatus.Kicked)
            {
                _chatsService.RemoveChatById(myChatMember.Chat.Id);
            }
        }

        private async Task MessageUpdate(Message message, Member member)
        {
            if (message.Text.ToLower().Contains("хрю"))
            {
                var hohol = _hoholService.GetActiveHoholByChatId(message.Chat.Id);
                if (hohol.IsActive())
                {
                    Random random = new Random();
                    int time = random.Next(2, 6);

                    _hoholService.AllowToWrite(hohol, time);

                    int i = random.Next(0, allowationMessages.Length);

                    var newDate = DateTime.Now.ToLocalTime().AddHours(time).ToString("HH:mm:ss");
                    await _client.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                        text: String.Format(allowationMessages[i], newDate),
                        replyParameters: message.MessageId
                    );
                }
            }
            else
            {
                var chat = _chatsService.GetChat(message.Chat.Id);
                if (chat == null) return;

                var hohol = new Hohol()
                {
                    MemberId = message.From.Id,
                    ChatId = message.Chat.Id,
                    Member = _memberService.GetMember(message.From.Id, message.Chat.Id)
                };
                if (_hoholService.IsActiveHohol(hohol))
                {
                    if (!hohol.IsAllowedToWrite())
                    {
                        await _client.DeleteMessageAsync(
                            chatId: hohol.ChatId,
                            messageId: message.MessageId
                        );

                        Random rand = new Random();
                        int i = rand.Next(0, claimMessages.Length);

                        await _client.SendTextMessageAsync(
                            chatId: hohol.ChatId,
                            text: $"@{hohol.Member.Username} {claimMessages[i]}"
                        );
                    }
                }
            }
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"An error occurred: {exception.Message}");
            return Task.CompletedTask;
        }

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
                var time = DateTime.Now.ToUniversalTime();

                _hoholService.ResetHohols();

                if (time.Second > 0) Thread.Sleep(waitingTime["second"]);
                if (time.Minute > 0) Thread.Sleep(waitingTime["minute"]);
                else Thread.Sleep(waitingTime["hour"]);
            }
        }
    }
}
