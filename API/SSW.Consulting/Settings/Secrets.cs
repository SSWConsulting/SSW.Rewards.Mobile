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

		public string CosmosDbEndPoint => _secrets.GetSecret(nameof(CosmosDbEndPoint));

		public string CosmosDbKey => _secrets.GetSecret(nameof(CosmosDbKey));

		public string DatabaseName => "SSWConsulting";

		public string ContentStorageConnectionString => _secrets.GetSecret(nameof(ContentStorageConnectionString));
	}
}
