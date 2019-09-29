using SSW.Consulting.Application.Interfaces;
using SSW.Consulting.Persistence;

namespace SSW.Consulting.WebAPI.Settings
{
	public class SSWConsultingDbContextSecrets : SSWConsultingDbContent.ISecrets
	{
		private readonly ISecretsProvider _secrets;

		public SSWConsultingDbContextSecrets(ISecretsProvider secrets)
		{
			_secrets = secrets;
		}

		public string CosmosDbEndPoint => _secrets.GetSecret(nameof(CosmosDbEndPoint));

		public string CosmosDbKey => _secrets.GetSecret(nameof(CosmosDbKey));

		public string DatabaseName => "SSWConsulting";
	}
}
