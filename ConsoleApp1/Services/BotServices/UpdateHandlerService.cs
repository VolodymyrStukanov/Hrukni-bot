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
        private readonly ILogger<UpdateHandlerService> logger;
        private readonly IHoholsService resetHoholService;
        private readonly IUnitOfWork unitOfWork;
        private readonly ITelegramBotClient botClient;
        private readonly IFilesService filesService;

        private readonly ICommonService<Member> memberService;
        private readonly ICommonService<Chat> chatService;
        private readonly ICommonService<Hohol> hoholService;

        private readonly string[] claimMessages;
        private readonly string[] allowationMessages;

        public UpdateHandlerService(ILogger<UpdateHandlerService> logger, ITelegramBotClient botClient,
            IHoholsService resetHoholService, IUnitOfWork unitOfWork, IConfiguration configuration, IFilesService filesService,
            ICommonService<Member> memberService, ICommonService<Chat> chatService, ICommonService<Hohol> hoholService)
        {
            this.botClient = botClient;
            this.resetHoholService = resetHoholService;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.memberService = memberService;
            this.chatService = chatService;
            this.hoholService = hoholService;
            this.filesService = filesService;

#if !Test
            claimMessages = configuration.GetSection("Messages").GetSection("ClaimMessages").Get<string[]>()!;
            allowationMessages = configuration.GetSection("Messages").GetSection("AllowationMessages").Get<string[]>()!;
#endif
        }

        public async Task HandleUpdate(Update update)
        {
            try
            {
#if !Test
                filesService.WriteUpdate(update);
#endif
                Member? member = await GetMemberFromUpdate(update);                

                if (member != null)
                {
                    UpdateMember(member);
                    if (update.Type == UpdateType.Message 
                        && update.Message != null 
                        && update.Message.Text == null)
                    {
                        await ChatMemberUpdate(update.Message);
                    }
                    else if (update.Type == UpdateType.Message
                        && update.Message != null
                        && update.Message.Text != null
                        && update.Message.From != null)
                    {
                        if (update.Message.Text[0] == '/')
                        {
                            await HandleCommand(member, update.Message);
                        }
                        else
                        {
                            await MessageUpdate(update.Message);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"------ Handling start ------ \nAn error occurred in HandleUpdate method");
#if !Test
                filesService.WriteErrorUpdate(update);
#endif
            }
        }

        private void UpdateMember(Member member)
        {
            try
            {
                var existingMember = memberService.Get(member.Id, member.ChatId);
                if (existingMember == null)
                {
                    memberService.Add(member);
                    unitOfWork.SaveChanges();
                }
                else
                {
                    existingMember.Username = member.Username;
                    existingMember.IsOwner = member.IsOwner;
                    unitOfWork.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred in UpdateMember method");
            }     
        }

        private async Task<Member?> GetMemberFromUpdate(Update update)
        {
            Member member;
            if (update.Type == UpdateType.Message && update.Message != null && update.Message.From != null)
            {
#if !Test
                ChatMember? memberInfo = await botClient.GetChatMemberAsync(update.Message.Chat.Id, update.Message.From.Id);
                member = new Member()
                {
                    Username = memberInfo.User.Username,
                    ChatId = update.Message.Chat.Id,
                    Id = update.Message.From.Id,
                    IsOwner = memberInfo.Status == ChatMemberStatus.Creator
                };
#else
                member = new Member()
                {
                    Username = "memberUsername",
                    ChatId = update.Message.Chat.Id,
                    Id = update.Message.From.Id,
                    IsOwner = (update.Type == UpdateType.MyChatMember && update.MyChatMember != null)
                     || (update.Message != null && update.Message.Text != null && update.Message.Text[0] == '/')
                };
#endif
            }
            else if (update.Type == UpdateType.MyChatMember && update.MyChatMember != null)
            {
#if !Test
                ChatMember? memberInfo = await botClient.GetChatMemberAsync(update.MyChatMember.Chat.Id, update.MyChatMember.From.Id);
                member = new Member()
                {
                    Username = memberInfo.User.Username,
                    ChatId = update.MyChatMember.Chat.Id,
                    Id = update.MyChatMember.From.Id,
                    IsOwner = memberInfo.Status == ChatMemberStatus.Creator
                };
                MyChatMemberUpdate(update.MyChatMember, member);
#else
                member = new Member()
                {
                    Username = "memberUsername",
                    ChatId = update.MyChatMember.Chat.Id,
                    Id = update.MyChatMember.From.Id,
                    IsOwner = (update.Type == UpdateType.MyChatMember && update.MyChatMember != null)
                     || (update.Message != null && update.Message.Text != null && update.Message.Text[0] == '/')
                };
                MyChatMemberUpdate(update.MyChatMember, member);
#endif
                return null;
            }
            else return null;

            return member;
        }

        private async Task HandleCommand(Member member, Message message)
        {
            try
            {
                var chat = chatService.Get(member.ChatId);
                if (chat == null)
                {
                    logger.LogError($"An error occurred in HandleCommand method. \nThe chat with id {member.ChatId} was not found");
                    return;
                }

                if (message.Text == "/start_hrukni" && member.IsOwner)
                {
                    chat.IsActive = true;
                    unitOfWork.SaveChanges();

#if !Test
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Хохлы...");
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Готовтесь хрюкать");
#endif
                }
                else if (message.Text == "/stop_hrukni" && member.IsOwner)
                {
                    chat.IsActive = false;
                    unitOfWork.SaveChanges();
                }
                else if (message.Text == "/reset_hohols" && member.IsOwner)
                {
                    var currentHohol = hoholService.Get(chat.Id);
                    if (currentHohol != null)
                    {
                        hoholService.Remove(currentHohol);
                        unitOfWork.SaveChanges();
                    }

                    var newHohol = resetHoholService.SelectNewHohol(chat.Id);
                    if(newHohol != null)
                    {
                        hoholService.Add(newHohol);
                        unitOfWork.SaveChanges();
                    }
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"An error occurred in HandleCommand method");
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
                        ChatMember? memberInfo = await botClient.GetChatMemberAsync(message.Chat.Id, newUser.Id);
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
                        memberService.Add(newMember);
                        unitOfWork.SaveChanges();
                    }
                }
                else if (message.LeftChatMember != null)
                {
                    memberService.Remove(message.LeftChatMember.Id, message.Chat.Id);
                    unitOfWork.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"An error occurred in ChatMemberUpdate method");
            }
        }

        private void MyChatMemberUpdate(ChatMemberUpdated myChatMember, Member member)
        {
            try
            {
                if (myChatMember.NewChatMember.Status == ChatMemberStatus.Left ||
                myChatMember.NewChatMember.Status == ChatMemberStatus.Kicked)
                {
                    chatService.Remove(myChatMember.Chat.Id);
                    unitOfWork.SaveChanges();
                }
                else if (myChatMember.NewChatMember.Status == ChatMemberStatus.Member)
                {
                    chatService.Add(new Chat()
                    {
                        Id = member.ChatId,
                        IsActive = false,
                    });
                    unitOfWork.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred in MyChatMemberUpdate method");
            }
        }

        private async Task MessageUpdate(Message message)
        {
            try
            {
                if (message.Date.ToLocalTime().CompareTo(DateTime.Now.AddMinutes(-2)) > 0)
                {
                    var hohol = hoholService.GetIncludingChilds(message.Chat.Id);
                    if(hohol != null)
                    {
                        if (message.Text!.ToLower().Contains("хрю"))
                        {
                            if (hohol.Member!.Id == message.From!.Id)
                            {
                                Random random = new Random();
                                int time = random.Next(2, 10);

                                hohol.EndWritingPeriod = DateTime.Now.ToUniversalTime().AddMinutes(time);
                                unitOfWork.SaveChanges();

#if !Test
                                int i = random.Next(0, allowationMessages.Length);

                                var newDate = hohol.EndWritingPeriod.ToLocalTime().ToString("HH:mm:ss");

                                await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                    text: string.Format(allowationMessages[i], newDate),
                                    replyParameters: message.MessageId
                                );
#endif
                            }
                        }
                        else
                        {
                            if (hohol.Member!.Id != message.From!.Id) return;

                            if (!hohol.IsAllowedToWrite())
                            {

#if !Test
                                await botClient.DeleteMessageAsync(
                                    chatId: hohol.ChatId,
                                    messageId: message.MessageId
                                );

                                Random rand = new Random();
                                int i = rand.Next(0, claimMessages.Length);

                                await botClient.SendTextMessageAsync(
                                    chatId: hohol.ChatId,
                                    text: $"@{hohol.Member.Username} {claimMessages[i]}"
                                );
#endif
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred in MessageUpdate method");
            }            
        }
    }
}
