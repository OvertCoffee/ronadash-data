using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace ronadash_data.Models
{
    public class RonaContext : DbContext
    {
        private readonly string host = "db";
        private readonly string database = Environment.GetEnvironmentVariable("POSTGRES_DB");
        private readonly string username = Environment.GetEnvironmentVariable("POSTGRES_USER");
        private readonly string password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        public DbSet<DbState> States { get; set; }
        public DbSet<County> Counties { get; set; }
        public DbSet<Record> Records { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Migrations/updates were acting up, so I'll just handle default values in Record.cs
            modelBuilder.Entity<Record>()
                .HasIndex(r => new { r.Country, r.Province_State, r.County, r.Last_Update })
                .IsUnique();
            modelBuilder.Entity<Record>().HasIndex(r => r.Country);
            modelBuilder.Entity<Record>().HasIndex(r => r.Province_State);
            modelBuilder.Entity<Record>().HasIndex(r => r.County);
            modelBuilder.Entity<Record>().HasIndex(r => r.Last_Update);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
            => optionsBuilder.UseNpgsql($"Host={host};Database={database};Username={username};Password={password}");
    }
}