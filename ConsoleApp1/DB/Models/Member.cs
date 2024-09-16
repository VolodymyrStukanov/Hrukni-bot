

using HrukniHohlinaBot.DB.Interfaces;

namespace HrukniHohlinaBot.DB.Models
{
    public class Member : IModel
    {
        public long Id { get; set; }
        public long ChatId { get; set; }
        public Chat Chat { get; set; }
        public string? Username { get; set; }
        public bool IsOwner { get; set; }

        public object[] GetKey()
        {
            return new object[]
            {
                Id,
                ChatId
            };
        }
    }
}
