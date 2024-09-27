using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
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
            try
            {
                _context.SaveChanges();
                DetachAll();
            }
            catch (Exception ex)
            {
                RollBack();
                throw ex;
            }
        }

        public void RollBack()
        {
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.State = EntityState.Detached;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.State = EntityState.Unchanged;
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Unchanged;
                }
            }
        }

        public void DetachAll()
        {
            var changes = _context.ChangeTracker.Entries();
            foreach (var change in changes)
            {
                if (change.Entity != null)
                {
                    _context.Entry(change.Entity).State = EntityState.Detached;
                }
            }
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _context.Database.BeginTransaction();
        }
    }
}
