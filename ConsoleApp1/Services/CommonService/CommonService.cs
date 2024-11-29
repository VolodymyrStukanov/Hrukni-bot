using HrukniHohlinaBot.DB;
using Microsoft.EntityFrameworkCore;

namespace HrukniHohlinaBot.Services.CommonService
{
    public abstract class CommonService<T, TKey>
        where T : class
    {
        protected readonly DbSet<T> dbSet;
        protected readonly ApplicationDbContext context;

        protected CommonService(ApplicationDbContext context)
        {
            dbSet = context.Set<T>();
            this.context = context;
        }

        protected virtual T Add(T model)
        {
            dbSet.Add(model);
            return model;
        }

        protected virtual void Update(T model)
        {
            dbSet.Update(model);
        }

        //protected virtual T? GetIncludingChildren(params object[] key)
        //{
        //    var entity = dbSet.Find(key);
        //    if (entity != null)
        //    {
        //        var entry = context.Entry(entity);
        //        foreach (var navigation in entry.Navigations)
        //        {
        //            navigation.Load();
        //        }
        //    }
        //    return entity;
        //}

        protected virtual T? Get(TKey key)
        {
            var entity = dbSet.Find(key);
            return entity;
        }

        protected virtual void Remove(TKey key)
        {
            var entity = Get(key);
            if (entity != null) dbSet.Remove(entity);
        }

        protected virtual void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        protected virtual IQueryable<T> GetAll()
        {
            return dbSet.AsNoTracking();
        }
    }
}
