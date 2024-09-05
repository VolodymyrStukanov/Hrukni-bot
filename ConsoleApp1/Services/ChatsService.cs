using ConsoleApp1.DB;
using ConsoleApp1.DB.Models;
using Microsoft.EntityFrameworkCore;
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
            _context.Entry(chat).State = EntityState.Detached;
        }
        public Chat? GetChat(long id)
        {
            var chat = _context.Chats.FirstOrDefault(x => x.Id == id);
            if(chat != null)
                _context.Entry(chat).State = EntityState.Detached;
            return chat;
        }
        public void RemoveChatById(long id)
        {
            _context.Chats.Remove(new Chat() { Id = id });
            _context.SaveChanges();
        }
    }
}
