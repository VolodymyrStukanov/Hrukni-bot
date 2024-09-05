using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace HrukniHohlinaBot.Services.HoholServices
{
    public class HoholService
    {
        object LockObject = new object();
        object ResetHoholLockObject = new object();

        ApplicationDbContext _context;
        public HoholService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Hohol? GetActiveHohol(long chatId)
        {
            lock (LockObject)
            {
                var hohol = _context.Hohols
                    .Include(x => x.Member)
                    .Include(x => x.Member.Chat)
                    .ToList()
                    .SingleOrDefault(x => x.ChatId == chatId && x.IsActive());
                if (hohol != null)
                    _context.Entry(hohol).State = EntityState.Detached;
                return hohol;
            }
        }

        public void ResetHoholForChat(long chatId)
        {
            lock (ResetHoholLockObject)
            {
                var hohols = _context.Hohols.Where(x => x.ChatId == chatId).ToArray();
                var members = _context.Members.Where(x => !x.IsOwner && x.ChatId == chatId).ToArray();

                if (members.Length > 0)
                {
                    Random rand = new Random();
                    var i = rand.Next(0, members.Length);

                    var existedHohol = hohols.SingleOrDefault(x => x.MemberId == members[i].Id && x.ChatId == chatId);
                    if (existedHohol != null)
                    {
                        existedHohol.AssignmentDate = DateTime.Now.ToUniversalTime();
                        existedHohol.EndWritingPeriod = DateTime.Now.ToUniversalTime();
                        UpdateHohol(existedHohol);
                    }
                    else
                    {
                        var hohol = new Hohol()
                        {
                            ChatId = chatId,
                            MemberId = members[i].Id,
                            AssignmentDate = DateTime.Now.ToUniversalTime(),
                            EndWritingPeriod = DateTime.Now.ToUniversalTime(),
                        };
                        AddHohol(hohol);
                    }
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

        private void AddHohol(Hohol hohol)
        {
            lock (LockObject)
            {
                _context.Hohols.Add(hohol);
                _context.SaveChanges();
                _context.Entry(hohol).State = EntityState.Detached;
            }
        }

        public void UpdateHohol(Hohol hohol)
        {
            lock (LockObject)
            {
                _context.Hohols.Update(hohol);
                _context.SaveChanges();
                _context.Entry(hohol).State = EntityState.Detached;
            }
        }
    }
}
