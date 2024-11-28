using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using EntityState = Microsoft.EntityFrameworkCore.EntityState;

namespace HrukniHohlinaBot.Services.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<UnitOfWork> logger;

        public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger)
        {
            this.context = context;
            this.logger = logger;
        }
        public void SaveChanges()
        {
            try
            {
                context.SaveChanges();
                DetachAll();
            }
            catch (Exception ex)
            {
                RollBack();
                logger.LogError(ex, "An error occurred in SaveChanges method in UnitOfWork");
            }
        }

        public void RollBack()
        {
            foreach (var entry in context.ChangeTracker.Entries())
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
            var changes = context.ChangeTracker.Entries();
            foreach (var change in changes.Select(x => x.Entity))
            {
                if (change != null)
                {
                    context.Entry(change).State = EntityState.Detached;
                }
            }
        }

        public IDbContextTransaction BeginTransaction()
        {
            return context.Database.BeginTransaction();
        }
    }
}
