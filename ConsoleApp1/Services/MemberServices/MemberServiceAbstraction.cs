using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.CommonService;
using System.Data.Entity;

namespace HrukniBot.Services.MemberServices
{
    public abstract class MemberServiceAbstraction : CommonService<Member, MemberKey>
    {
        protected MemberServiceAbstraction(ApplicationDbContext context) 
            : base(context)
        {
        }

        protected Member? GetIncludingChildren(MemberKey key) => context.Members
            .Include(x => x.Chat)
            .FirstOrDefault(x => x.Id == key.Id && x.ChatId == key.ChatId);

        protected override Member? Get(MemberKey key)
        {
            return dbSet.Find(key.Id, key.ChatId);
        }


    }
}
