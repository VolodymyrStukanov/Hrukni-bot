
using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.CommonService;
using Microsoft.EntityFrameworkCore;

namespace HrukniBot.Services.HoholServices
{
    public class HoholServiceAbstraction : CommonService<Hohol, long>
    {
        public HoholServiceAbstraction(ApplicationDbContext context) : base(context)
        {
        }

        protected virtual Hohol? GetIncludingChildren(long ChatId) => context.Hohols
            .Include(x => x.Member)
            .Include(x => x.Chat)
            .FirstOrDefault(x => x.ChatId == ChatId);
    }
}
