using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Extensions.Configuration;

namespace HrukniHohlinaBot.Services.BotServices
{
    public class UpdateHandlerService : IUpdateHandlerService
    {
        private readonly ILogger<TelegramBotService> _logger;
        private IResetHoholService _hoholService;
        private IUnitOfWork _unitOfWork;
        private ITelegramBotClient _botClient;
        private IConfiguration _configuration;

        private string[] _claimMessages;
        private string[] _allowationMessages;

        public UpdateHandlerService(ILogger<TelegramBotService> logger, ITelegramBotClient botClient,
            IResetHoholService hoholService, IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _botClient = botClient;
            _hoholService = hoholService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
            _claimMessages = configuration.GetSection("Messages").GetSection("ClaimMessages").Get<string[]>();
            _allowationMessages = configuration.GetSection("Messages").GetSection("AllowationMessages").Get<string[]>();
        }


        public async Task HandleUpdate(Update update)
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

            if (_unitOfWork.ChatService.Get(member.ChatId) == null) return;

            if (_unitOfWork.MemberService.Get(member.Id, member.ChatId) == null)
            {
                _unitOfWork.MemberService.Add(member);
                _unitOfWork.Commit();
                return;
            }

            if (update.Type == UpdateType.Message && update.Message != null && update.Message.Text == null)
            {
                await ChatMemberUpdate(update.Message);
            }
            else if (update.Type == UpdateType.Message && update.Message != null && update.Message.Text != null && update.Message.From != null)
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
            if (update.Type == UpdateType.Message && update.Message != null && update.Message.From != null)
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

            ChatMember? memberInfo = await _botClient.GetChatMemberAsync(chatId, memberId);
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
                var chat = _unitOfWork.ChatService.Get(message.Chat.Id);
                if (chat == null)
                {
                    _unitOfWork.ChatService.Add(new DB.Models.Chat()
                    {
                        Id = message.Chat.Id,
                    });
                    _unitOfWork.Commit();
                }

                await _botClient.SendTextMessageAsync(message.Chat.Id, $"Хохлы...");
                await _botClient.SendTextMessageAsync(message.Chat.Id, $"Готовтесь хрюкать");
            }
            else if (message.Text == "/stop_hrukni" && member.IsOwner)
            {
                _unitOfWork.ChatService.Remove(message.Chat.Id);
                _unitOfWork.Commit();
            }
            else if (message.Text == "/reset_hohols" && member.IsOwner)
            {
                _hoholService.ResetHoholForChat(message.Chat.Id);
                _unitOfWork.Commit();
            }
        }

        private async Task ChatMemberUpdate(Message message)
        {
            if (message.NewChatMembers != null)
            {
                foreach (var newUser in message.NewChatMembers)
                {
                    if (newUser.IsBot) continue;

                    ChatMember? memberInfo = await _botClient.GetChatMemberAsync(message.Chat.Id, newUser.Id);
                    Member newMember = new Member()
                    {
                        Username = newUser.Username,
                        ChatId = message.Chat.Id,
                        Id = newUser.Id,
                        IsOwner = memberInfo.Status == ChatMemberStatus.Creator
                    };
                    _unitOfWork.MemberService.Add(newMember);
                    _unitOfWork.Commit();
                }
            }
            else if (message.LeftChatMember != null)
            {
                _unitOfWork.MemberService.Remove(message.LeftChatMember.Id, message.Chat.Id);
                _unitOfWork.Commit();
            }
        }

        private async Task MyChatMemberUpdate(ChatMemberUpdated myChatMember, Member member)
        {
            if (myChatMember.NewChatMember.Status == ChatMemberStatus.Left ||
            myChatMember.NewChatMember.Status == ChatMemberStatus.Kicked)
            {
                _unitOfWork.ChatService.Remove(myChatMember.Chat.Id);
                _unitOfWork.Commit();
            }
        }

        private async Task MessageUpdate(Message message, Member member)
        {
            if (message.Date.ToLocalTime().CompareTo(DateTime.Now.AddMinutes(-2)) > 0)
            {
                if (message.Text.ToLower().Contains("хрю")
                && !message.Text.ToLower().Contains("не")
                && !message.Text.ToLower().Contains("ні")
                && !message.Text.ToLower().Contains("нє"))
                {
                    var hohol = _unitOfWork.HoholService.GetIncludingChilds(message.Chat.Id);
                    if (hohol.Member.Id == message.From.Id)
                    {
                        if (hohol != null)
                        {
                            Random random = new Random();
                            int time = random.Next(2, 10);

                            hohol.EndWritingPeriod = DateTime.Now.ToUniversalTime().AddMinutes(time);
                            _unitOfWork.HoholService.Update(hohol);
                            _unitOfWork.Commit();

                            int i = random.Next(0, _allowationMessages.Length);

                            var newDate = hohol.EndWritingPeriod.ToLocalTime().ToString("HH:mm:ss");
                            await _botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                                text: string.Format(_allowationMessages[i], newDate),
                                replyParameters: message.MessageId
                            );
                        }
                    }
                }
                else
                {
                    var hohol = _unitOfWork.HoholService.GetIncludingChilds(message.Chat.Id);
                    if (hohol == null || hohol.Member.Id != message.From.Id) return;

                    if (!hohol.IsAllowedToWrite())
                    {
                        await _botClient.DeleteMessageAsync(
                            chatId: hohol.ChatId,
                            messageId: message.MessageId
                        );

                        Random rand = new Random();
                        int i = rand.Next(0, _claimMessages.Length);

                        await _botClient.SendTextMessageAsync(
                            chatId: hohol.ChatId,
                            text: $"@{hohol.Member.Username} {_claimMessages[i]}"
                        );
                    }
                }
            }
        }
    }
}
