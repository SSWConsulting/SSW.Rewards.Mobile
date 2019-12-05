using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Infrastructure;
using SSW.Rewards.Persistence;

namespace SSW.Rewards.WebAPI.Settings
{
    public class Secrets : SSWConsultingDbContext.ISecrets,
		CloudBlobClientProvider.ISecrets
	{
		private readonly ISecretsProvider _secrets;

		public Secrets(ISecretsProvider secrets)
		{
			_secrets = secrets;
		}

		public string SqlConnectionString => _secrets.GetSecret(nameof(SqlConnectionString));

		public string ContentStorageConnectionString => _secrets.GetSecret(nameof(ContentStorageConnectionString));
	}
}
