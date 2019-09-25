using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Interfaces;
using SSW.Consulting.Domain.Entities;
using System;

namespace SSW.Consulting.Persistence
{
    public class SSWConsultingDbContent : DbContext, ISSWConsultingDbContent
    {

        public SSWConsultingDbContent()
        {
        }
        public DbSet<StaffMember> StaffMembers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseCosmos(
                    "https://localhost:8081",
                    "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    databaseName: "OrdersDB");
        // TODO: get from appsettings

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultContainer("Staff");

            modelBuilder.Entity<StaffMember>()
                .ToContainer("Staff");

            //modelBuilder.Entity<Order>()
            //    .HasPartitionKey(o => o.PartitionKey);
        }

    }
}
