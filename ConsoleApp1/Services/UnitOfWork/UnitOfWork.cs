using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrukniHohlinaBot.Services.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context, ICommonService<Chat> chatService, 
            ICommonService<Member> memberService, ICommonService<Hohol> hoholService)
        {
            _context = context;
            ChatService = chatService;
            MemberService = memberService;
            HoholService = hoholService;
        }

        private ICommonService<Chat> _chatService;
        public ICommonService<Chat> ChatService
        {
            get { return _chatService; }
            private set { _chatService ??= value; }
        }

        private ICommonService<Member> _memberService;
        public ICommonService<Member> MemberService
        {
            get { return _memberService; }
            private set { _memberService ??= value; }
        }

        private ICommonService<Hohol> _hoholService;
        public ICommonService<Hohol> HoholService
        {
            get { return _hoholService; }
            private set { _hoholService ??= value; }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        public void Commit()
        {
            _context.SaveChanges();
            DetachAll();
        }
        public void DetachAll()
        {
            var changes = _context.ChangeTracker.Entries();
            foreach (var change in changes)
            {
                if(change.Entity!= null)
                {
                    _context.Entry(change.Entity).State = EntityState.Detached;
                }
            }
        }
    }
}
