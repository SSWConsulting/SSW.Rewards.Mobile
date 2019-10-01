using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Interfaces;
using SSW.Consulting.Domain.Entities;

namespace SSW.Consulting.Persistence
{
	public class SSWConsultingDbContent : DbContext, ISSWConsultingDbContent
    {
		public interface ISecrets
		{
			string CosmosDbEndPoint { get; }
			string CosmosDbKey { get; }
			string DatabaseName { get; }
		}

		private readonly ISecrets _secrets;

		public SSWConsultingDbContent(ISecrets secrets)
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
                .ToContainer("Staff");

            //modelBuilder.Entity<Order>()
            //    .HasPartitionKey(o => o.PartitionKey);
        }

    }
}
