using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.DB.Models
{
    public class Hohol
    {
        public int Id { get; set; }
        public long MemberId { get; set; }
        public long ChatId { get; set; }
        public Member Member { get; set; }
        public DateTime AssignmentDate { get; set; }
        public DateTime EndWritingPeriod { get; set; }

        public bool IsActive()
        {
            var dateTime = DateTime.Now.ToUniversalTime();
            if (AssignmentDate.AddDays(1).AddHours(1).CompareTo(dateTime) < 0) return false;
            return true;
        }

        public bool IsAllowedToWrite()
        {
            var dateTime = DateTime.Now.ToUniversalTime();
            if (EndWritingPeriod.CompareTo(dateTime) < 0) return false;
            return true;
        }
    }
}
