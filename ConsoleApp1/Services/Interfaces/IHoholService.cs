using HrukniHohlinaBot.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrukniHohlinaBot.Services.Interfaces
{
    internal interface IHoholService
    {
        public void ResetHohols();
        public void ResetHoholForChat(long chatId);
        public Hohol? GetActiveHohol(long chatId);
        public void UpdateHohol(Hohol hohol);
    }
}
