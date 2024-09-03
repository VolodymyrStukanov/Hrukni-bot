using ConsoleApp1.DB;
using ConsoleApp1.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ConsoleApp1.Services
{
    public class HoholService
    {
        ApplicationDbContext _context;
        public HoholService(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool IsActiveHohol(Hohol model)
        {
            var hohol = _context.Hohols.ToList().SingleOrDefault(x => x.ChatId == model.ChatId && x.Username == model.Username && x.IsActive());
            return hohol != null;
        }

        public Hohol? GetActiveHoholByChatId(long chatId)
        {
            return _context.Hohols.ToList().SingleOrDefault(x => x.ChatId == chatId && x.IsActive());
        }

        public void ResetHoholForChat(long chatId)
        {
            var members = _context.Members;
            var hohols = _context.Hohols;
            var currentHohol = hohols.FirstOrDefault(x => x.ChatId == chatId);
            Member[] chatMembers;
            if (currentHohol == null)
            {
                chatMembers = members.ToArray();

            }
            else if (!currentHohol.IsActive())
            {
                chatMembers = members.Where(x => x.ChatId == chatId && x.Username != currentHohol.Username).ToArray();
            }
            else return;

            Random rand = new Random();
            var i = rand.Next(0, chatMembers.Length);
            if (chatMembers.Length > 0)
            {
                var hohol = new Hohol()
                {
                    ChatId = chatId,
                    Username = chatMembers[i].Username,
                    AssignmentDate = DateTime.Now,
                    EndWritingPeriod = DateTime.Now,
                };
                if (hohols.SingleOrDefault(x => x.Username == hohol.Username && x.ChatId == hohol.ChatId) != null)
                {
                    UpdateHohol(hohol);
                }
                else
                {
                    AddHohol(hohol);
                }
            }
        }

        public void ResetHohols()
        {
            var chats = _context.Chats;
            foreach (var chat in chats)
            {
                ResetHoholForChat(chat.Id);
            }
        }

        private void AddHohol(Hohol hohol)
        {
            _context.Hohols.Add(hohol);
            _context.SaveChanges();
        }

        private void UpdateHohol(Hohol hohol)
        {
            _context.Hohols.Update(hohol);
            _context.SaveChanges();
        }
    }
}
