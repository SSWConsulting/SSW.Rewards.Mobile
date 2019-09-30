using Microsoft.Extensions.Configuration;
using SSW.Consulting.Infrastructure;

namespace SSW.Consulting.WebAPI.Settings
{

	public class AppSettings :
		KeyVaultSecretsProvider.ISettings
	{
		private readonly IConfiguration _config;

		public AppSettings(IConfiguration config)
		{
			_config = config;
		}

		public string KeyVaultUrl => _config[nameof(KeyVaultUrl)];

		public AzureAdB2CSettings AzureAdB2C
		{
			get
			{
				var b2cSettings = new AzureAdB2CSettings();
				_config.Bind("AzureAdB2C", b2cSettings);
				return b2cSettings;
			}
		}
	}
}
