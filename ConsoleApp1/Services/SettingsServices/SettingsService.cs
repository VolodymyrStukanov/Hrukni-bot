using HrukniHohlinaBot.DB;
using HrukniHohlinaBot.DB.Models;

namespace HrukniHohlinaBot.Services.SettingsServices
{
    public class SettingsService
    {
        ApplicationDbContext _context;
        public SettingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Settings? GetSettingsById(int id)
        {
            return _context.Settings.SingleOrDefault(x => x.Id == id);
        }
    }
}
