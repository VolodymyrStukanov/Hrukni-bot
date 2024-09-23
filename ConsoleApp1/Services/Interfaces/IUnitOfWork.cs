using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace HrukniHohlinaBot.Services.Interfaces
{
    public interface IUnitOfWork
    {
        public void SaveChanges();
        public void DetachAll();
        public IDbContextTransaction BeginTransaction();
    }
}
