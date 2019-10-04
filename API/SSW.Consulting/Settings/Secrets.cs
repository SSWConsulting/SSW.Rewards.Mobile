using SSW.Consulting.Application.Interfaces;
using SSW.Consulting.Infrastructure;
using SSW.Consulting.Persistence;

namespace SSW.Consulting.WebAPI.Settings
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
