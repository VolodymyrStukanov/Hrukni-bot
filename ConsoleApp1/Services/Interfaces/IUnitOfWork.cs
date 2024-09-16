namespace HrukniHohlinaBot.Services.Interfaces
{
    internal interface IUnitOfWork
    {
        public void Commit();
        public void Dispose();
        public void DetachAll();
    }
}
