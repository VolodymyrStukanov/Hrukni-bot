using ConsoleApp1.DB;
using ConsoleApp1.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Services
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
