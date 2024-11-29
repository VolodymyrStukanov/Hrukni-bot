
using HrukniHohlinaBot.DB.Models;
using System.Runtime.CompilerServices;

namespace HrukniBot.Services.MemberServices
{
    internal interface IMemberService
    {
        public Member AddMember(Member model);

        public void UpdateMember(Member model);

        public IQueryable<Member> GetAllMembers();

        public Member? GetMember(long Id, long ChatId);

        public Member? GetMemberIncludingChildren(long Id, long ChatId);

        public void RemoveMember(long Id, long ChatId);

        public void RemoveMember(Member entity);
    }
}
