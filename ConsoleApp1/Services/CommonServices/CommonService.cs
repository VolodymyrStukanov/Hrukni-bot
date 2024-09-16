using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Interfaces;
using HrukniHohlinaBot.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrukniHohlinaBot.Services.CommonServices
{
    public class CommonService<T> : IEntitiesService<T> where T : class, IModel
    {
        private readonly DbSet<T> _dbSet;

        public CommonService(ApplicationDbContext context)
        {
            _dbSet = context.Set<T>();
        }

        public T Create(T model)
        {
            _dbSet.Add(model);
            return model;
        }

        public void Update(T model)
        {
            _dbSet.Update(model);
        }

        public T? Get(params object[] key)
        {
            var entity = _dbSet.Find(key);
            if(entity != null)
                _dbSet.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public void Remove(params object[] key)
        {
            var entity = Get(key);
            if (entity != null) _dbSet.Remove(entity);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

    }
}
