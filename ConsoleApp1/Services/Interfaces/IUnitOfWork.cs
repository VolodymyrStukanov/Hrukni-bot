using HrukniHohlinaBot.DB.Models;

namespace HrukniHohlinaBot.Services.Interfaces
{
    public interface IUnitOfWork
    {
        public ICommonService<Chat> ChatService { get; }
        public ICommonService<Member> MemberService { get; }
        public ICommonService<Hohol> HoholService { get; }

        public void Commit();
        public void Dispose();
        public void DetachAll();
    }
}
