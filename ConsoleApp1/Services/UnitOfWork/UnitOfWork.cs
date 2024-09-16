using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.CommonServices;
using HrukniHohlinaBot.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrukniHohlinaBot.Services.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context,
            CommonService<Chat> chatService, CommonService<Member> memberService)
        {
            _context = context;
            ChatService = chatService;
            MemberService = memberService;
        }

        private CommonService<Chat> _chatService;
        public CommonService<Chat> ChatService
        {
            get { return _chatService; }
            private set { _chatService ??= value; }
        }

        private CommonService<Member> _memberService;
        public CommonService<Member> MemberService
        {
            get { return _memberService; }
            private set { _memberService ??= value; }
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
