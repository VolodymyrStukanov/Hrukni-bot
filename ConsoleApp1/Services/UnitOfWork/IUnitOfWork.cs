using Microsoft.EntityFrameworkCore.Storage;

namespace HrukniBot.Services.UnitOfWork
{
    public interface IUnitOfWork
    {
        public void SaveChanges();
        public void DetachAll();
        public void RollBack();
        public IDbContextTransaction BeginTransaction();
    }
}
