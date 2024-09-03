using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.DB.Models
{
    public class Member
    {
        public string Username { get; set; }
        public long ChatId { get; set; }
        public Chat Chat { get; set; }
    }
}
