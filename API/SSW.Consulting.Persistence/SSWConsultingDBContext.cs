using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using SSW.Consulting.Application.Interfaces;
using SSW.Consulting.Domain.Entities;
using System.Linq;

namespace SSW.Consulting.Persistence
{
	public class SSWConsultingDbContext : DbContext, ISSWConsultingDbContext
    {
		public interface ISecrets
		{
			string CosmosDbEndPoint { get; }
			string CosmosDbKey { get; }
			string DatabaseName { get; }
		}

		private readonly ISecrets _secrets;

		public SSWConsultingDbContext(ISecrets secrets)
        {
			_secrets = secrets;
		}

        public DbSet<StaffMember> StaffMembers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseCosmos(
                    accountEndpoint: _secrets.CosmosDbEndPoint,
                    accountKey: _secrets.CosmosDbKey,
                    databaseName: _secrets.DatabaseName);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultContainer("Staff");

            modelBuilder.Entity<StaffMember>()
                .ToContainer("Staff")
                .Property(s => s.Id).HasValueGenerator<GuidValueGenerator>();
            modelBuilder.Entity<StaffMember>()
                .OwnsMany<Skill>(s => s.Skills);

            //modelBuilder.Entity<Order>()
            //    .HasPartitionKey(o => o.PartitionKey);
        }

    }
}
