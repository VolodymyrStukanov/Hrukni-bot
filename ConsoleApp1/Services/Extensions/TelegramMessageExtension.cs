
using System.CodeDom;
using Telegram.Bot.Types;

namespace ConsoleApp1.Services.Extensions
{
    public static class TelegramMessageExtension
    {
        public static bool IsRecentMessage(this Message message){
            return message.Date.ToLocalTime().CompareTo(DateTime.Now.AddMinutes(-2)) > 0;
        }
    }
}