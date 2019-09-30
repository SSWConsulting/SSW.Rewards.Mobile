using Microsoft.Extensions.Configuration;
using SSW.Consulting.Application.Interfaces;
using System.Threading.Tasks;

namespace SSW.Consulting.Infrastructure
{
	/// <summary>
	/// FOR LOCAL DEVELOPMENT ONLY - Alternative secrets provider if you want to load secrets from the local appsettings instead of KeyVault
	/// </summary>
	public class LocalSecretsProvider : ISecretsProvider
	{
		private readonly IConfiguration _config;

		public LocalSecretsProvider(IConfiguration config)
		{
			_config = config;
		}

		public string GetSecret(string secretName)
		{
			return _config[secretName];
		}

		public async Task<string> GetSecretAsync(string secretName)
		{
			return await Task.FromResult(_config[secretName]);
		}
	}
}
