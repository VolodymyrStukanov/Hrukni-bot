using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace HrukniHohlinaBot.Services.ResetHoholServices
{
    public class ResetHoholService : IResetHoholService
    {
        readonly object LockObject = new object();

        private readonly ILogger<ResetHoholService> logger;
        private readonly IUnitOfWork unitOfWork;

        private readonly ICommonService<Chat> chatService;
        private readonly ICommonService<Hohol> hoholService;
        private readonly ICommonService<Member> memberService;
        public ResetHoholService(ILogger<ResetHoholService> logger, IUnitOfWork unitOfWork, 
            ICommonService<Chat> chatService, ICommonService<Hohol> hoholService, ICommonService<Member> memberService)
        {
            this.logger = logger;
            this.unitOfWork = unitOfWork;
            this.chatService = chatService;
            this.hoholService = hoholService;
            this.memberService = memberService;
        }

        public Hohol? SelectNewHohol(long chatId)
        {
            lock (LockObject)
            {
                var members = memberService.GetAll().Where(x => !x.IsOwner && x.ChatId == chatId).ToArray();
                if(members.Length == 0) return null;

                Random rand = new Random();
                var i = rand.Next(0, members.Length);

                Hohol newHohol = new Hohol()
                {
                    ChatId = chatId,
                    MemberId = members[i].Id,
                    AssignmentDate = DateTime.Now.ToUniversalTime(),
                    EndWritingPeriod = DateTime.Now.ToUniversalTime(),
                };
                return newHohol;
            }
        }

        public void ResetHohols()
        {
            lock (LockObject)
            {
                var chats = chatService.GetAll().ToList();
                foreach (var chatId in chats.Select(x => x.Id))
                {
                    using var transaction = unitOfWork.BeginTransaction();
                    try
                    {
                        var currentHohol = hoholService.Get(chatId);
                        if (currentHohol != null)
                        {
                            hoholService.Remove(currentHohol.ChatId);
                            unitOfWork.SaveChanges();
                        }

                        var newHohol = SelectNewHohol(chatId);
                        if (newHohol != null)
                        {
                            hoholService.Add(newHohol);
                            unitOfWork.SaveChanges();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"An error occurred in ResetHohols method");
                        transaction.Rollback();
                    }
                }
            }
        }
    }
}
