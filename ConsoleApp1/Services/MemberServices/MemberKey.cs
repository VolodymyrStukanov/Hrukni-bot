
namespace HrukniBot.Services.MemberServices
{
    internal class MemberKey
    {
        public long Id { get; set; }
        public long ChatId { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not MemberKey key) return false;
            return Id == key.Id && ChatId == key.ChatId;
        }
        public override int GetHashCode() => HashCode.Combine(Id, ChatId);
    }
}
