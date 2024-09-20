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
        public ResetHoholService(ILogger<TelegramBotService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public void ResetHoholForChat(long chatId)
        {
            lock (LockObject)
            {
                try
                {
                    var hohols = _unitOfWork.HoholService.GetAll().Where(x => x.ChatId == chatId).ToArray();
                    var members = _unitOfWork.MemberService.GetAll().Where(x => !x.IsOwner && x.ChatId == chatId).ToArray();

                    if (members.Length > 0)
                    {
                        Random rand = new Random();
                        var i = rand.Next(0, members.Length);

                        var currentHohols = hohols.Where(x => x.IsActive() || x.ChatId == chatId);
                        if (currentHohols.Count() != 0)
                        {
                            _unitOfWork.HoholService.RemoveRange(currentHohols);
                            _unitOfWork.Commit();
                        }

                        var hohol = new Hohol()
                        {
                            ChatId = chatId,
                            MemberId = members[i].Id,
                            AssignmentDate = DateTime.Now.ToUniversalTime(),
                            EndWritingPeriod = DateTime.Now.ToUniversalTime(),
                        };
                        _unitOfWork.HoholService.Add(hohol);
                        _unitOfWork.Commit();
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError($"An error occurred in ResetHoholForChat method: {ex.Message}\nThe ChatId = {chatId}");
                    _unitOfWork.Dispose();
                }                
            }
        }

        public void ResetHohols()
        {
            lock (LockObject)
            {
                var chats = _unitOfWork.ChatService.GetAll().ToList();
                foreach (var chat in chats)
                {
                    ResetHoholForChat(chat.Id);
                }
            }
        }
    }
}
