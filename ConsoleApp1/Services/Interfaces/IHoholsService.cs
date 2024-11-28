
using HrukniHohlinaBot.DB.Models;

namespace HrukniHohlinaBot.Services.Interfaces
{
    public interface IHoholsService
    {
        public void ResetHohols();
        public Hohol? SelectNewHohol(long chatId);
    }
}
