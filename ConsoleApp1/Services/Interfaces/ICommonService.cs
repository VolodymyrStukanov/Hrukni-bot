﻿namespace HrukniHohlinaBot.Services.Interfaces
{
    public interface ICommonService<T>
    {
        public T Add(T model);
        public void Update(T model);
        public void Remove(T model);
        public void Remove(params object[] key);
        public void RemoveRange(IEnumerable<T> entities);
        public T? Get(params object[] key);
        public T? GetIncludingChilds(params object[] key);
        public IQueryable<T> GetAll();
    }
}
