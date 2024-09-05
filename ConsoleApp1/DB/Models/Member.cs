

namespace ConsoleApp1.DB.Models
{
    public class Member
    {
        public long Id { get; set; }
        public long ChatId { get; set; }
        public Chat Chat { get; set; }
        public string? Username { get; set; }
        public bool IsOwner { get; set; }
    }
}
