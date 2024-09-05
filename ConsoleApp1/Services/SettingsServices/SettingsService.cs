using ConsoleApp1.DB;
using ConsoleApp1.DB.Models;

namespace ConsoleApp1.Services.SettingsServices
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
