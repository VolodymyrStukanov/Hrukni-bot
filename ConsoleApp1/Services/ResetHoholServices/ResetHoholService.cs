using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.BotServices;
using HrukniHohlinaBot.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace HrukniHohlinaBot.Services.ResetHoholServices
{
    public class ResetHoholService : IResetHoholService
    {
        object LockObject = new object();

        private readonly ILogger<TelegramBotService> _logger;
        private IUnitOfWork _unitOfWork;

        private ICommonService<Chat> _chatService;
        private ICommonService<Hohol> _hoholService;
        private ICommonService<Member> _memberService;
        public ResetHoholService(ILogger<TelegramBotService> logger, IUnitOfWork unitOfWork, 
            ICommonService<Chat> chatService, ICommonService<Hohol> hoholService, ICommonService<Member> memberService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _chatService = chatService;
            _hoholService = hoholService;
            _memberService = memberService;
        }

        public Hohol? SelectNewHohol(long chatId)
        {
            lock (LockObject)
            {
                var members = _memberService.GetAll().Where(x => !x.IsOwner && x.ChatId == chatId).ToArray();
                Hohol newHohol = null;
                if (members.Length > 0)
                {
                    Random rand = new Random();
                    var i = rand.Next(0, members.Length);

                    newHohol = new Hohol()
                    {
                        ChatId = chatId,
                        MemberId = members[i].Id,
                        AssignmentDate = DateTime.Now.ToUniversalTime(),
                        EndWritingPeriod = DateTime.Now.ToUniversalTime(),
                    };
                }
                return newHohol;
            }
        }

        public void ResetHohols()
        {
            lock (LockObject)
            {
                var chats = _chatService.GetAll().ToList();
                foreach (var chat in chats)
                {
                    using var transaction = _unitOfWork.BeginTransaction();
                    try
                    {
                        var currentHohol = _hoholService.Get(chat.Id);
                        if (currentHohol != null)
                        {
                            _hoholService.Remove(currentHohol.ChatId);
                            _unitOfWork.SaveChanges();
                        }

                        var newHohol = SelectNewHohol(chat.Id);
                        if (newHohol != null)
                        {
                            _hoholService.Add(newHohol);
                            _unitOfWork.SaveChanges();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An error occurred in ResetHohols method: {ex.Message}");
                        transaction.Rollback();
                    }
                }
            }
        }
    }
}
