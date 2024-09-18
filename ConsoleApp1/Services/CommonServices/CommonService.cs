using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrukniHohlinaBot.Services.CommonServices
{
    public class CommonService<T> : ICommonService<T> where T : class
    {
        private readonly DbSet<T> _dbSet;
        private readonly ApplicationDbContext _context;

        public CommonService(ApplicationDbContext context)
        {
            _dbSet = context.Set<T>();
            _context = context;
        }

        public T Add(T model)
        {
            _dbSet.Add(model);
            return model;
        }

        public void Update(T model)
        {
            _dbSet.Update(model);
        }

        public T? GetIncludingChilds(params object[] key)
        {
            var entity = _dbSet.Find(key);
            if (entity != null)
            {
                var entry = _context.Entry(entity);
                foreach (var navigation in entry.Navigations)
                {
                    navigation.Load();
                }
                _dbSet.Entry(entity).State = EntityState.Detached;
            }
            return entity;
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

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet;
        }
    }
}
