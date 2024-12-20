﻿
namespace HrukniHohlinaBot.DB.Models
{
    public class Hohol
    {
        public long MemberId { get; set; }
        public Member? Member { get; set; }
        public long ChatId { get; set; }
        public Chat? Chat { get; set; }
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
