using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.ChatsServices;
using HrukniHohlinaBot.Services.HoholServices;
using HrukniHohlinaBot.Services.MemberServices;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HrukniHohlinaBot.Bot
{
    public class BotEventHandler
    {
        private ChatsService _chatsService;
        private HoholService _hoholService;
        private MemberService _memberService;
        private TelegramBotClient _client;
        public BotEventHandler(TelegramBotClient client, ChatsService chatsService,
            HoholService hoholService, MemberService memberService)
        {
            _chatsService = chatsService;
            _hoholService = hoholService;
            _memberService = memberService;
            _client = client;
        }

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
            @"Хороший хохол, Можеш хрюкать до {0}",
            @"Умничка, Хрюкаешь до {0}",
            @"Молодец, можеш розслабиться до {0}",
            @"До {0} можеш на совей свинячей балакать"
        };

        public async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Member? member = await GetNewMember(update);
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

        private async Task<Member?> GetNewMember(Update update)
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

        private async Task HandleCommand(Member member, Message message)
        {
            if (message.Text[0] != '/') return;
            if (message.Text == "/start_hrukni" && member.IsOwner)
            {
                var chat = _chatsService.GetChat(message.Chat.Id);
                if (chat == null)
                {
                    _chatsService.AddChat(new DB.Models.Chat()
                    {
                        Id = message.Chat.Id,
                    });
                }

                await _client.SendTextMessageAsync(message.Chat.Id, $"Хохлы...");
                var hohol = _hoholService.GetActiveHohol(message.Chat.Id);
                if (hohol == null)
                {
                    _hoholService.ResetHoholForChat(message.Chat.Id);
                    await _client.SendTextMessageAsync(message.Chat.Id, $"Готовтесь хрюкать");
                }
            }
            else if (message.Text == "/stop_hrukni" && member.IsOwner)
            {
                var chat = _chatsService.GetChat(message.Chat.Id);
                if (chat != null)
                {
                    _chatsService.RemoveChatById(chat);
                }
            }
            else if (message.Text == "/reset_hohols" && member.IsOwner)
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
                var member = _memberService.GetMember(message.LeftChatMember.Id, message.Chat.Id);
                if(member != null)
                {
                    _memberService.RemoveMember(member);
                }
            }
        }

        private async Task MyChatMemberUpdate(ChatMemberUpdated myChatMember, Member member)
        {
            if (myChatMember.NewChatMember.Status == ChatMemberStatus.Left ||
            myChatMember.NewChatMember.Status == ChatMemberStatus.Kicked)
            {
                var chat = _chatsService.GetChat(myChatMember.Chat.Id);
                if (chat != null)
                {
                    _chatsService.RemoveChatById(chat);
                }
            }
        }

        private async Task MessageUpdate(Message message, Member member)
        {
            if (message.Date.CompareTo(DateTime.Now.AddMinutes(-2)) > 0)
            {
                if (message.Text.ToLower().Contains("хрю")
                && !message.Text.ToLower().Contains("не")
                && !message.Text.ToLower().Contains("ні")
                && !message.Text.ToLower().Contains("нє"))
                {
                    var hohol = _hoholService.GetActiveHohol(message.Chat.Id);
                    if (hohol.Member.Id == message.From.Id)
                    {
                        if (hohol != null)
                        {
                            Random random = new Random();
                            int time = random.Next(2, 10);

                            hohol.EndWritingPeriod = DateTime.Now.ToUniversalTime().AddMinutes(time);
                            _hoholService.UpdateHohol(hohol);

                            int i = random.Next(0, allowationMessages.Length);

                            var newDate = hohol.EndWritingPeriod.ToLocalTime().ToString("HH:mm:ss");
                            await _client.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                                text: String.Format(allowationMessages[i], newDate),
                                replyParameters: message.MessageId
                            );
                        }
                    }
                }
                else
                {
                    var hohol = _hoholService.GetActiveHohol(message.Chat.Id);
                    if (hohol == null || hohol.Member.Id != message.From.Id) return;

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

        public Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"An error occurred: {exception.Message}");
            return Task.CompletedTask;
        }

    }
}
