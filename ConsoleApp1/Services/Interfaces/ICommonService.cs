namespace HrukniHohlinaBot.Services.Interfaces
{
    public interface ICommonService<T>
    {
        public T Create(T model);
        public void Update(T model);
        public void Remove(T model);
        public void Remove(params object[] key);
        public T? Get(params object[] key);
        public T? GetIncludingChilds(params object[] key);
        public IQueryable<T> GetAll();
    }
}
