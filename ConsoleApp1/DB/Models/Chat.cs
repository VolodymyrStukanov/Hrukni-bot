

using HrukniHohlinaBot.DB.Interfaces;

namespace HrukniHohlinaBot.DB.Models
{
    public class Chat : IModel
    {
        public long Id { get; set; }
        public object[] GetKey()
        {
            return new object[]
            {
                Id
            };
        }
    }
}
