using HrukniHohlinaBot.DB.Models;

namespace HrukniHohlinaBot.Services.Interfaces
{
    public interface IUnitOfWork
    {
        public void Commit();
        public void Rollback();
        public void DetachAll();
    }
}
