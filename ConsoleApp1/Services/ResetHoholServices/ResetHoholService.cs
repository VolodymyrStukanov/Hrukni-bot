using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.BotServices;
using HrukniHohlinaBot.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace HrukniHohlinaBot.Services.ResetHoholServices
{
    public class ResetHoholService : IResetHoholService
    {
        object LockObject = new object();

        private ApplicationDbContext _context;
        private readonly ILogger<TelegramBotService> _logger;
        private IUnitOfWork _unitOfWork;
        public ResetHoholService(ApplicationDbContext context, ILogger<TelegramBotService> logger,
            IUnitOfWork unitOfWork)
        {
            _context = context;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public void ResetHoholForChat(long chatId)
        {
            lock (LockObject)
            {
                var hohols = _unitOfWork.HoholService.GetAll().Where(x => x.ChatId == chatId).ToArray();
                var members = _unitOfWork.MemberService.GetAll().Where(x => !x.IsOwner && x.ChatId == chatId).ToArray();

                if (members.Length > 0)
                {
                    Random rand = new Random();
                    var i = rand.Next(0, members.Length);

                    var currentHohol = hohols.SingleOrDefault(x => x.IsActive());
                    if (currentHohol != null)
                    {
                        _unitOfWork.HoholService.Remove(currentHohol);
                        _unitOfWork.Commit();
                    }

                    var hohol = new Hohol()
                    {
                        ChatId = chatId,
                        MemberId = members[i].Id,
                        AssignmentDate = DateTime.Now.ToUniversalTime(),
                        EndWritingPeriod = DateTime.Now.ToUniversalTime(),
                    };
                    _unitOfWork.HoholService.Create(hohol);
                    _unitOfWork.Commit();
                }
            }
        }

        public void ResetHohols()
        {
            lock (LockObject)
            {
                var chats = _context.Chats.ToList();
                foreach (var chat in chats)
                {
                    ResetHoholForChat(chat.Id);
                }
            }
        }
    }
}
