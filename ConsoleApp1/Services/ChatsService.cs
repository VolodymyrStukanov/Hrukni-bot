using ConsoleApp1.DB;
using ConsoleApp1.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Services
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
            if(_context.Chats.FirstOrDefault(x => x.Id == chat.Id) != null) return;

            _context.Chats.Add(chat);
            _context.SaveChanges();
        }
        public Chat? GetChat(long id)
        {
            return _context.Chats.FirstOrDefault(x => x.Id == id);
        }
    }
}
