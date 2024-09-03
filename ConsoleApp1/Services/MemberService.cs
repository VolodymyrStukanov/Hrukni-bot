using ConsoleApp1.DB;
using ConsoleApp1.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _context.Members.Add(member);
            _context.SaveChanges();
        }

        public Member? GetMember(string username, long chatId)
        {
            return _context.Members.SingleOrDefault(x => x.Username == username && x.ChatId == chatId);
        }
    }
}
