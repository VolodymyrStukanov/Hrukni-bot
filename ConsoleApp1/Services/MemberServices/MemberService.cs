using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace HrukniHohlinaBot.Services.MemberServices
{
    public class MemberService
    {
        ApplicationDbContext _context;
        public MemberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddMember(Member member)
        {
            if (_context.Members.FirstOrDefault(x => x.Id == member.Id && x.ChatId == member.ChatId) != null) return;
            _context.Members.Add(member);
            _context.SaveChanges();
            _context.Entry(member).State = EntityState.Detached;
        }

        public Member? GetMember(long id, long chatId)
        {
            var member = _context.Members
                .Include(x => x.Chat)
                .SingleOrDefault(x => x.Id == id && x.ChatId == chatId);
            if (member != null)
                _context.Entry(member).State = EntityState.Detached;
            return member;
        }

        public void RemoveMember(Member member)
        {
            _context.Members.Remove(member);
            _context.SaveChanges();
        }
    }
}
