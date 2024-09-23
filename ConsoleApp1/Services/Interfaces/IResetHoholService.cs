
using HrukniHohlinaBot.DB.Models;

namespace HrukniHohlinaBot.Services.Interfaces
{
    public interface IResetHoholService
    {
        public void ResetHohols();
        public Hohol? SelectNewHohol(long chatId);
    }
}
