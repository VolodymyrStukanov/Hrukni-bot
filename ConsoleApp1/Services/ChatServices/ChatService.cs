using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;
using HrukniHohlinaBot.Services.CommonService;

namespace HrukniBot.Services.ChatServices
{
    public class ChatService : CommonService<Chat, long>, IChatService
    {
        public ChatService(ApplicationDbContext context) 
            : base(context)
        {
        }

        public Chat AddChat(Chat model)
        {            
            return this.Add(model);
        }

        public void UpdateChat(Chat model)
        {
            this.Update(model);
        }

        public Chat? GetChat(long id)
        {
            return this.Get(id);
        }

        public IQueryable<Chat> GetAllChats()
        {
            return GetAll();
        }

        public void RemoveChat(long key)
        {
            Remove(key);
        }

        public void RemoveChat(Chat entity)
        {
            Remove(entity);
        }
    }
}
