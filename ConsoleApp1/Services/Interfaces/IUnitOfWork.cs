using Microsoft.EntityFrameworkCore.Storage;

namespace HrukniHohlinaBot.Services.Interfaces
{
    public interface IUnitOfWork
    {
        public void SaveChanges();
        public void DetachAll();
        public void RollBack();
        public IDbContextTransaction BeginTransaction();
    }
}
