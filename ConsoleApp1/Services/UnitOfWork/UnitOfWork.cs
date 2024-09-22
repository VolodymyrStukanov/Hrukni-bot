using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using EntityState = Microsoft.EntityFrameworkCore.EntityState;

namespace HrukniHohlinaBot.Services.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }
        public void SaveChanges()
        {
            _context.SaveChanges();
            DetachAll();
        }
        public void DetachAll()
        {
            var changes = _context.ChangeTracker.Entries();
            foreach (var change in changes)
            {
                if(change.Entity!= null)
                {
                    _context.Entry(change.Entity).State = EntityState.Detached;
                }
            }
        }

        public IDbTransaction BeginTransaction()
        {
            var transaction = _context.Database.BeginTransaction();

            return transaction.GetDbTransaction();
        }
    }
}
