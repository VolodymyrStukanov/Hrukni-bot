namespace HrukniHohlinaBot.Services.Interfaces
{
    internal interface IEntitiesService<T>
    {
        public T Create(T model);
        public void Update(T model);
        public void Remove(T model);
        public void Remove(object[] key);
        public T? Get(object[] key);
    }
}
