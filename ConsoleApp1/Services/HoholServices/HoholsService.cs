using HrukniBot.Services.ChatServices;
using HrukniBot.Services.HoholServices;
using HrukniBot.Services.MemberServices;
using HrukniBot.Services.UnitOfWork;
using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using Microsoft.Extensions.Logging;

namespace HrukniHohlinaBot.Services.HoholServices
{
    public class HoholsService : HoholServiceAbstraction, IHoholsService
    {
        private readonly object LockObject = new object();

        private readonly ILogger<HoholsService> logger;
        private readonly IUnitOfWork unitOfWork;

        private readonly IChatService chatService;
        private readonly IMemberService memberService;

        public HoholsService(
            ApplicationDbContext context,
            ILogger<HoholsService> logger, 
            IUnitOfWork unitOfWork,
            IChatService chatService,
            IMemberService memberService)
            : base(context)
        {
            this.logger = logger;
            this.unitOfWork = unitOfWork;
            this.chatService = chatService;
            this.memberService = memberService;
        }

        public Hohol AddHohol(Hohol model)
        {
            return this.Add(model);
        }

        public void UpdateHohol(Hohol model)
        {
            this.Update(model);
        }

        public Hohol? GetHohol(long key)
        {
            return this.Get(key);
        }

        public Hohol? GetHoholIncludingChildren(long ChatId)
        {
            return this.GetIncludingChildren(ChatId);
        }

        public IQueryable<Hohol> GetAllHohols()
        {
            return this.GetAll();
        }

        public void RemoveHohol(long ChatId)
        {
            this.Remove(ChatId);
        }

        public void RemoveHohol(Hohol entity)
        {
            this.Remove(entity);
        }

        public Hohol? SelectNewHohol(long chatId)
        {
            lock (LockObject)
            {
                var members = memberService.GetAllMembers().Where(x => !x.IsOwner && x.ChatId == chatId).ToArray();
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
                var chats = chatService.GetAllChats().ToList();
                foreach (var chatId in chats.Select(x => x.Id))
                {
                    using var transaction = unitOfWork.BeginTransaction();
                    try
                    {
                        var currentHohol = GetHohol(chatId);
                        if (currentHohol != null)
                        {
                            this.RemoveHohol(currentHohol.ChatId);
                            unitOfWork.SaveChanges();
                        }

                        var newHohol = SelectNewHohol(chatId);
                        if (newHohol != null)
                        {
                            AddHohol(newHohol);
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
