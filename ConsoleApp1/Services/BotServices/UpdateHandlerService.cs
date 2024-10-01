using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Extensions.Configuration;
using Chat = HrukniHohlinaBot.DB.Models.Chat;

namespace HrukniHohlinaBot.Services.BotServices
{
    public class UpdateHandlerService : IUpdateHandlerService
    {
        private readonly ILogger<TelegramBotService> _logger;
        private IResetHoholService _resetHoholService;
        private IUnitOfWork _unitOfWork;
        private ITelegramBotClient _botClient;
        private IConfiguration _configuration;
        private IFilesService _filesService;

        private ICommonService<Member> _memberService;
        private ICommonService<Chat> _chatService;
        private ICommonService<Hohol> _hoholService;

        private string[] _claimMessages;
        private string[] _allowationMessages;

        public UpdateHandlerService(ILogger<TelegramBotService> logger, ITelegramBotClient botClient,
            IResetHoholService resetHoholService, IUnitOfWork unitOfWork, IConfiguration configuration, IFilesService filesService,
            ICommonService<Member> memberService, ICommonService<Chat> chatService, ICommonService<Hohol> hoholService)
        {
            _botClient = botClient;
            _resetHoholService = resetHoholService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
            _memberService = memberService;
            _chatService = chatService;
            _hoholService = hoholService;
            _filesService = filesService;

#if !Test
            _claimMessages = configuration.GetSection("Messages").GetSection("ClaimMessages").Get<string[]>();
            _allowationMessages = configuration.GetSection("Messages").GetSection("AllowationMessages").Get<string[]>();
#endif
        }


        public async Task HandleUpdate(Update update)
        {
            try
            {
#if !Test
                _filesService.WriteUpdate(update);
#endif
                Member? member = await GetMemberFromUpdate(update);
                if (update.Type == UpdateType.MyChatMember && update.MyChatMember != null)
                {
                    await MyChatMemberUpdate(update.MyChatMember, member);
                    return;
                }

                await UpdateMember(member);
                if (member != null)
                {
                    if (update.Type == UpdateType.Message && update.Message != null && update.Message.Text == null)
                    {
                        await ChatMemberUpdate(update.Message);
                    }
                    else if (update.Type == UpdateType.Message
                        && update.Message != null
                        && update.Message.Text != null
                        && update.Message.From != null)
                    {
                        if (update.Type == UpdateType.Message
                        && update.Message != null
                        && update.Message.Text != null
                        && update.Message.Text[0] == '/')
                        {
                            await HandleCommand(member, update.Message);
                        }
                        else await MessageUpdate(update.Message);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"------ Handling start ------ \nAn error occurred in HandleUpdate method: {ex.Message}");
#if !Test
                _filesService.WriteErrorUpdate(update);
#endif
            }
        }

        private async Task UpdateMember(Member member)
        {
            if (member == null) return;

            try
            {
                var existingMember = _memberService.Get(member.Id, member.ChatId);
                if (existingMember == null)
                {
                    _memberService.Add(member);
                    _unitOfWork.SaveChanges();
                }
                else
                {
                    existingMember.Username = member.Username;
                    existingMember.IsOwner = member.IsOwner;
                    _unitOfWork.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred in UpdateMember method: {ex.Message}");
                throw ex;
            }     
        }

        private async Task<Member?> GetMemberFromUpdate(Update update)
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

#if !Test
            ChatMember? memberInfo = await _botClient.GetChatMemberAsync(chatId, memberId);
            Member member = new Member()
            {
                Username = memberInfo.User.Username,
                ChatId = chatId,
                Id = memberId,
                IsOwner = memberInfo.Status == ChatMemberStatus.Creator
            };
#else
            Member member = new Member()
            {
                Username = "memberUsername",
                ChatId = chatId,
                Id = memberId,
                IsOwner = (update.Type == UpdateType.MyChatMember && update.MyChatMember != null)
                 || (update.Message != null && update.Message.Text != null && update.Message.Text[0] == '/')
            };
#endif

            return member;
        }

        private async Task HandleCommand(Member member, Message message)
        {
            try
            {
                var chat = _chatService.Get(member.ChatId);
                if (message.Text == "/start_hrukni" && member.IsOwner)
                {
                    chat.IsActive = true;
                    _unitOfWork.SaveChanges();

#if !Test
                    await _botClient.SendTextMessageAsync(message.Chat.Id, $"Хохлы...");
                    await _botClient.SendTextMessageAsync(message.Chat.Id, $"Готовтесь хрюкать");
#endif
                }
                else if (message.Text == "/stop_hrukni" && member.IsOwner)
                {
                    chat.IsActive = false;
                    _unitOfWork.SaveChanges();
                }
                else if (message.Text == "/reset_hohols" && member.IsOwner)
                {
                    var currentHohol = _hoholService.Get(member.ChatId);
                    if (currentHohol != null)
                    {
                        _hoholService.Remove(currentHohol);
                        _unitOfWork.SaveChanges();
                    }

                    var newHohol = _resetHoholService.SelectNewHohol(member.ChatId);
                    if(newHohol != null)
                    {
                        _hoholService.Add(newHohol);
                        _unitOfWork.SaveChanges();
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"An error occurred in HandleCommand method: {ex.Message}");
                throw ex;
            }
        }

        private async Task ChatMemberUpdate(Message message)
        {
            try
            {
                if (message.NewChatMembers != null)
                {
                    foreach (var newUser in message.NewChatMembers)
                    {
                        if (newUser.IsBot) continue;

#if !Test
                        ChatMember? memberInfo = await _botClient.GetChatMemberAsync(message.Chat.Id, newUser.Id);
                        Member newMember = new Member()
                        {
                            Username = newUser.Username,
                            ChatId = message.Chat.Id,
                            Id = newUser.Id,
                            IsOwner = memberInfo.Status == ChatMemberStatus.Creator
                        };
#else
                        Member newMember = new Member()
                        {
                            Username = newUser.Username,
                            ChatId = message.Chat.Id,
                            Id = newUser.Id,
                            IsOwner = false
                        };
#endif
                        _memberService.Add(newMember);
                        _unitOfWork.SaveChanges();
                    }
                }
                else if (message.LeftChatMember != null)
                {
                    _memberService.Remove(message.LeftChatMember.Id, message.Chat.Id);
                    _unitOfWork.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"An error occurred in ChatMemberUpdate method: {ex.Message}");
            }
        }

        private async Task MyChatMemberUpdate(ChatMemberUpdated myChatMember, Member member)
        {
            try
            {
                if (myChatMember.NewChatMember.Status == ChatMemberStatus.Left ||
                myChatMember.NewChatMember.Status == ChatMemberStatus.Kicked)
                {
                    _chatService.Remove(myChatMember.Chat.Id);
                    _unitOfWork.SaveChanges();
                }
                else if (myChatMember.NewChatMember.Status == ChatMemberStatus.Member)
                {
                    _chatService.Add(new Chat()
                    {
                        Id = member.ChatId,
                        IsActive = false,
                    });
                    _unitOfWork.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred in MyChatMemberUpdate method: {ex.Message}");
                throw ex;
            }
        }

        private async Task MessageUpdate(Message message)
        {
            try
            {
                if (message.Date.ToLocalTime().CompareTo(DateTime.Now.AddMinutes(-2)) > 0)
                {
                    var hohol = _hoholService.GetIncludingChilds(message.Chat.Id);
                    if (message.Text.ToLower().Contains("хрю")
                    && !message.Text.ToLower().Contains("не")
                    && !message.Text.ToLower().Contains("ні")
                    && !message.Text.ToLower().Contains("нє"))
                    {
                        if (hohol.Member.Id == message.From.Id)
                        {
                            if (hohol != null)
                            {
                                Random random = new Random();
                                int time = random.Next(2, 10);

                                hohol.EndWritingPeriod = DateTime.Now.ToUniversalTime().AddMinutes(time);
                                _unitOfWork.SaveChanges();

#if !Test
                                int i = random.Next(0, _allowationMessages.Length);

                                var newDate = hohol.EndWritingPeriod.ToLocalTime().ToString("HH:mm:ss");

                                await _botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                    text: string.Format(_allowationMessages[i], newDate),
                                    replyParameters: message.MessageId
                                );
#endif
                            }
                        }
                    }
                    else
                    {
                        if (hohol == null || hohol.Member.Id != message.From.Id) return;

                        if (!hohol.IsAllowedToWrite())
                        {

#if !Test
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
#endif
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred in MessageUpdate method: {ex.Message}");
                throw ex;
            }            
        }
    }
}
