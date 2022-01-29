using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Database
{
    public class DatabaseContext : DbContext
    {
        // 시세 데이터
        // 옵션데이터 + 가격
        public DatabaseContext() : base() { }

        public DbSet<PriceInfo> PriceInfo { get; set; }
        public DbSet<ItemOption> ItemOptions { get; set; }
        public DbSet<PriceInfo2ItemOption> PriceInfoItemOption { get; set; }
        public DbSet<IncomeReport> IncomeReports { get; set; }

        public override int SaveChanges()
        {
            SetDateTime();
            return base.SaveChanges();
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetDateTime();
            return base.SaveChangesAsync(cancellationToken);
        }
        private void SetDateTime() {
            var entries = ChangeTracker.Entries()
           .Where(e => e.Entity is ModelBase && (
                   e.State == EntityState.Added
                   || e.State == EntityState.Modified));
            foreach (var item in entries)
            {
                (item.Entity as ModelBase).UpdatedAt = DateTime.Now;
                if (item.State == EntityState.Added)
                    (item.Entity as ModelBase).CreatedAt = DateTime.Now;
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DatabaseContext"].ConnectionString;
            if (connectionString.Contains("Data Source="))
                optionsBuilder.UseSqlite(connectionString);
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // M to M 시작
            modelBuilder.Entity<PriceInfo2ItemOption>()
                .HasKey(p2i => new { p2i.ItemOptionId, p2i.PriceInfoId });

            modelBuilder.Entity<PriceInfo2ItemOption>()
                .HasOne(p2i => p2i.PriceInfo)
                .WithMany(pi => pi.PriceInfo2ItemOptions)
                .HasForeignKey(p2i => p2i.PriceInfoId);

            modelBuilder.Entity<PriceInfo2ItemOption>()
                .HasOne(p2i => p2i.ItemOption)
                .WithMany(io => io.PriceInfo2ItemOptions)
                .HasForeignKey(p2i => p2i.ItemOptionId);
            // M to M 끝

            modelBuilder.Entity<PriceInfo>()
                .Property(pi => pi.Server)
                .HasConversion<string>();

            modelBuilder.Entity<ItemOption>()
                .Property(io=>io.Stat)
                .HasConversion<string>();
            
            modelBuilder.Entity<IncomeReport>()
                .Property(pi => pi.Server)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
