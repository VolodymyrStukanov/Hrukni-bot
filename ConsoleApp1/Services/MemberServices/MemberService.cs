using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;

namespace HrukniBot.Services.MemberServices
{
    internal class MemberService : MemberServiceAbstraction, IMemberService
    {
        public MemberService(ApplicationDbContext context) 
            : base(context)
        {
        }

        public Member AddMember(Member model)
        {            
            return this.Add(model);
        }

        public void UpdateMember(Member model)
        {
            this.Update(model);
        }

        public Member? GetMember(long Id, long ChatId)
        {
            return this.Get(new MemberKey() { ChatId = ChatId, Id = Id});
        }

        public Member? GetMemberIncludingChildren(long Id, long ChatId)
        {
            return this.GetIncludingChildren(new MemberKey() { Id = Id, ChatId = ChatId });
        }

        public IQueryable<Member> GetAllMembers()
        {
            return this.GetAll();
        }

        public void RemoveMember(long Id, long ChatId)
        {
            this.Remove(new MemberKey() { ChatId = ChatId, Id = Id});
        }

        public void RemoveMember(Member entity)
        {
            this.Remove(entity);
        }
    }
}
