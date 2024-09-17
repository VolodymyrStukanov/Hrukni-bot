

using HrukniHohlinaBot.DB.Interfaces;

namespace HrukniHohlinaBot.DB.Models
{
    public class Chat : IDataTableModel
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
