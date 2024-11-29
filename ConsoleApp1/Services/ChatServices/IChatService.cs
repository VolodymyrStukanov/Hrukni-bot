using HrukniHohlinaBot.DB.Models;

namespace HrukniBot.Services.ChatServices
{
    internal interface IChatService
    {
        public Chat AddChat(Chat model);

        public void UpdateChat(Chat model);

        public IQueryable<Chat> GetAllChats();

        public Chat? GetChat(long key);

        public void RemoveChat(long key);

        public void RemoveChat(Chat entity);
    }
}
