using ConsoleApp1.DB;
using ConsoleApp1.DB.Models;
using Microsoft.EntityFrameworkCore;
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
            var hohol = _context.Hohols.ToList().SingleOrDefault(x => x.ChatId == model.ChatId && x.MemberId == model.MemberId && x.IsActive());
            if (hohol == null) 
                _context.Entry(hohol).State = EntityState.Detached;
            return hohol != null;
        }

        public Hohol? GetActiveHoholByChatId(long chatId)
        {
            var hohol = _context.Hohols.ToList().SingleOrDefault(x => x.ChatId == chatId && x.IsActive());
            if(hohol != null)
                _context.Entry(hohol).State = EntityState.Detached;
            return hohol;
        }

        public void AllowToWrite(Hohol model, int time)
        {
            var hohol = _context.Hohols.FirstOrDefault(x => x.ChatId == model.ChatId && x.MemberId == model.MemberId);
            if (hohol != null)
            {
                hohol.EndWritingPeriod = DateTime.Now.ToUniversalTime().AddHours(time);
                _context.Hohols.Update(hohol);
                _context.SaveChanges();
                _context.Entry(hohol).State = EntityState.Detached;
            }
        }

        public async void ResetHoholForChat(long chatId)
        {
            var members = await _context.Members.Where(x => !x.IsOwner).ToListAsync();
            var hohols = await _context.Hohols.ToListAsync();
            var currentHohol = hohols.FirstOrDefault(x => x.ChatId == chatId);
            Member[] chatMembers;
            if (currentHohol == null)
            {
                chatMembers = members.ToArray();
            }
            else if (!currentHohol.IsActive())
            {
                chatMembers = members.Where(x => x.ChatId == chatId && x.Id != currentHohol.MemberId).ToArray();
            }
            else return;

            if (chatMembers.Length > 0)
            {
                Random rand = new Random();
                var i = rand.Next(0, chatMembers.Length);
                if (chatMembers.Length > 0)
                {
                    var hohol = new Hohol()
                    {
                        ChatId = chatId,
                        MemberId = chatMembers[i].Id,
                        AssignmentDate = DateTime.Now.ToUniversalTime(),
                        EndWritingPeriod = DateTime.Now.ToUniversalTime(),
                    };
                    if (hohols.SingleOrDefault(x => x.MemberId == hohol.MemberId && x.ChatId == hohol.ChatId) != null)
                    {
                        UpdateHohol(hohol);
                    }
                    else
                    {
                        AddHohol(hohol);
                    }
                }
            }
        }

        public void ResetHohols()
        {
            var chats = _context.Chats.ToList();
            foreach (var chat in chats)
            {
                ResetHoholForChat(chat.Id);
            }
        }

        private void AddHohol(Hohol hohol)
        {
            _context.Hohols.Add(hohol);
            _context.SaveChanges();
            _context.Entry(hohol).State = EntityState.Detached;
        }

        private void UpdateHohol(Hohol hohol)
        {
            _context.Hohols.Update(hohol);
            _context.SaveChanges();
            _context.Entry(hohol).State = EntityState.Detached;
        }
    }
}
