using System;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Common.Interfaces;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace SSW.Rewards.Infrastructure
{
    public class KeyVaultSecretsProvider : ISecretsProvider
	{
		private readonly ISettings _settings;
		private readonly IDistributedCache _cache;
		private readonly ILogger<KeyVaultSecretsProvider> _log;
		private readonly KeyVaultClient _client;

		public interface ISettings
		{
			string KeyVaultUrl { get; }
			int SecretCacheTimeoutMinutes { get; }
		}

		// TODO: Add IMemoryCache or something so that we can cache secrets for a minute or 5, otherwise keyvault could get hammered too much
		public KeyVaultSecretsProvider(ILogger<KeyVaultSecretsProvider> log, ISettings settings, IKeyVaultClientProvider provider, IDistributedCache cache)
		{
			_settings = settings;
			_cache = cache;
			_log = log;
			_client = provider.GetClient();
		}

		public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken)
		{
			try
			{
				string cachedSecret = await _cache.GetStringAsync(secretName, cancellationToken);
				if (!string.IsNullOrWhiteSpace(cachedSecret))
				{
					return cachedSecret;
				}

				SecretBundle secret = await _client.GetSecretAsync(_settings.KeyVaultUrl, secretName, cancellationToken);
				if (!string.IsNullOrWhiteSpace(secret.Value))
				{
					await _cache.SetStringAsync(secretName, secret.Value, new DistributedCacheEntryOptions()
					{
						SlidingExpiration = TimeSpan.FromHours(_settings.SecretCacheTimeoutMinutes)
					}, cancellationToken);
				}

				return secret.Value;
			}
#if DEBUG
            catch (AzureServiceTokenProviderException)
            {
                _log.LogInformation("ATTENTION DEVELOPER: You need may need to install the Azure CLI. Then try restart this application.");
                throw;
            }
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
			return GetSecretAsync(secretName, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
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
