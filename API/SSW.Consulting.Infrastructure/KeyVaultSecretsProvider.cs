using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SSW.Consulting.Application.Interfaces;
using System.Net;
using System.Threading.Tasks;

namespace SSW.Consulting.Infrastructure
{
	public class KeyVaultSecretsProvider : ISecretsProvider
	{
		private readonly ISettings _settings;
		private readonly ILogger<KeyVaultSecretsProvider> _log;
		private readonly KeyVaultClient _client;

		public interface ISettings
		{
			string KeyVaultUrl { get; }
		}

		// TODO: Add IMemoryCache or something so that we can cache secrets for a minute or 5, otherwise keyvault could get hammered too much
		public KeyVaultSecretsProvider(ILogger<KeyVaultSecretsProvider> log, ISettings settings, IKeyVaultClientProvider provider)
		{
			_settings = settings;
			_log = log;
			_client = provider.GetClient();
		}

		public async Task<string> GetSecretAsync(string secretName)
		{
			try
			{
				SecretBundle secret = await _client.GetSecretAsync(_settings.KeyVaultUrl, secretName);
				return secret.Value;
			}
#if DEBUG
			catch (KeyVaultErrorException ex) when (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
			{
				_log.LogInformation("ATTENTION DEVELOPER: You need to log into Azure using your own credentials. Using the Azure CLI, run 'az login', provide your credentials and then run 'az account set -s <YOUR SUBSCRIPTION ID>'. Then try restart this application.");
				throw;
			}
#endif
			catch (KeyVaultErrorException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
			{
				_log.LogWarning(ex, $"The secret {secretName} could not be found in the keyvault.");
				throw;
			}
			catch (KeyVaultErrorException ex)
			{
				_log.LogError(ex, $"Something went wrong: {ex.Message}");
				throw;
			}
		}

		public string GetSecret(string secretName)
		{
			return GetSecretAsync(secretName).ConfigureAwait(false).GetAwaiter().GetResult();
		}
	}

	public interface IKeyVaultClientProvider
	{
		KeyVaultClient GetClient();
	}

	public class KeyVaultClientProvider : IKeyVaultClientProvider
	{
		public KeyVaultClient GetClient()
		{
			var azureServiceTokenProvider = new AzureServiceTokenProvider();
			return new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
		}
	}
}
