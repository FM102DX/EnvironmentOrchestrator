using ActivityScheduler.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.DataAccess
{
    public class EFSqliteDbContext : DbContext
    {
        public string DbPath { get; private set; }

        public string Guid { get; private set; }

        public EFSqliteDbContext(string dbStorageDirectory)
        {
            DbPath = Path.Combine(dbStorageDirectory, "ActivitySchedulerData.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath};");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            /*
             * 
             * modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).HasMaxLength(50);
                entity.Property(e => e.FullName).HasMaxLength(30);
            });

            */

            modelBuilder.Entity<SettingStorageUnit>();
            modelBuilder.Entity<Activity>();
            modelBuilder.Entity<Batch>();

            base.OnModelCreating(modelBuilder);
        }


    }
}
