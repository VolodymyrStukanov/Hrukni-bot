using ConsoleApp1.DB;
using ConsoleApp1.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp1.Services.ChatsServices
{
    public class ChatsService
    {

        ApplicationDbContext _context;
        public ChatsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddChat(Chat chat)
        {
            _context.Chats.Add(chat);
            _context.SaveChanges();
            _context.Entry(chat).State = EntityState.Detached;
        }
        public Chat? GetChat(long id)
        {
            var chat = _context.Chats.FirstOrDefault(x => x.Id == id);
            if (chat != null)
                _context.Entry(chat).State = EntityState.Detached;
            return chat;
        }
        public void RemoveChatById(Chat chat)
        {
            _context.Chats.Remove(chat);
            _context.SaveChanges();
        }
    }
}
