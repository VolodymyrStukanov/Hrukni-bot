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

        public void ResetHoholForChat(long chatId)
        {
            lock (LockObject)
            {
                using var transaction = _unitOfWork.BeginTransaction();
                try
                {
                    var hohols = _hoholService.GetAll().Where(x => x.ChatId == chatId).ToArray();
                    var members = _memberService.GetAll().Where(x => !x.IsOwner && x.ChatId == chatId).ToArray();

                    if (members.Length > 0)
                    {
                        Random rand = new Random();
                        var i = rand.Next(0, members.Length);

                        var currentHohols = hohols.Where(x => x.IsActive() || x.ChatId == chatId);
                        if (currentHohols.Count() != 0)
                        {
                            _hoholService.RemoveRange(currentHohols);
                            _unitOfWork.SaveChanges();
                        }

                        var hohol = new Hohol()
                        {
                            ChatId = chatId,
                            MemberId = members[i].Id,
                            AssignmentDate = DateTime.Now.ToUniversalTime(),
                            EndWritingPeriod = DateTime.Now.ToUniversalTime(),
                        };
                        _hoholService.Add(hohol);
                        _unitOfWork.SaveChanges();
                    }
                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    _logger.LogError($"An error occurred in ResetHoholForChat method: {ex.Message}\nThe ChatId = {chatId}");
                    transaction.Rollback();
                }                
            }
        }

        public void ResetHohols()
        {
            lock (LockObject)
            {
                var chats = _chatService.GetAll().ToList();
                foreach (var chat in chats)
                {
                    ResetHoholForChat(chat.Id);
                }
            }
        }
    }
}
