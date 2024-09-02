using ConsoleApp1.DB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.DB
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Settings>(entity =>
            {
                entity.HasData(new Settings()
                {
                    Id = -1,
                    Token = "6376204287:AAELhYb3664qx-QWbyAUW8oK0psZuVhwT9c"
                });
            });
        }

        public DbSet<Settings> Settings { get; set; }

    }

}
