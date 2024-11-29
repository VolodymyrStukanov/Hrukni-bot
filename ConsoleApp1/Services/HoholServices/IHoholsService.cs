using HrukniHohlinaBot.DB.Models;

namespace HrukniBot.Services.HoholServices
{
    internal interface IHoholsService
    {
        public void ResetHohols();

        public Hohol? SelectNewHohol(long chatId);

        public Hohol AddHohol(Hohol model);

        public void UpdateHohol(Hohol model);

        public IQueryable<Hohol> GetAllHohols();

        public Hohol? GetHohol(long key);

        public Hohol? GetHoholIncludingChildren(long ChatId);

        public void RemoveHohol(long key);

        public void RemoveHohol(Hohol entity);
    }
}
