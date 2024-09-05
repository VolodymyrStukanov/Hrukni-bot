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
    public class MemberService
    {
        ApplicationDbContext _context;
        public MemberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddMember(Member member)
        {
            if (_context.Members.FirstOrDefault(x => x.Id == member.Id && x.ChatId == member.ChatId) != null) return;
            _context.Members.Add(member);
            _context.SaveChanges();
            _context.Entry(member).State = EntityState.Detached;
        }

        public Member? GetMember(long id, long chatId)
        {
            var member = _context.Members.SingleOrDefault(x => x.Id == id && x.ChatId == chatId);
            if (member != null)
                _context.Entry(member).State = EntityState.Detached;
            return member;
        }

        public void RemoveMember(long id, long chatId)
        {
            _context.Members.Remove(new Member() { ChatId = chatId, Id = id });
            _context.SaveChanges();
        }
    }
}
