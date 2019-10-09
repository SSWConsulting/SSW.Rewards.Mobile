using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SSW.Consulting.Infrastructure;

namespace SSW.Consulting.WebAPI.Settings
{
	/// <summary>
	/// Strongly type AppSettings resolved from the IConfiguration config providers (JSON, Env, Memory, etc.)
	///
	/// This class supports optional and required settings.
	///
	/// If a required setting is missing from the configuration, then the <see cref="MissingAppSettingException"/> is thrown.
	///		e.g. GetValue("RequiredValue"); // throws an exception
	/// 
	/// If an optional settings is missing from the configuration, then the coded default value will be returned.
	///		e.g. GetValue("OptionalIntSetting", 5); // returns 5
	///		e.g. GetValue("OptionalBooleanSetting", false); // returns false
	/// 
	/// </summary>
	public class AppSettings :
		KeyVaultSecretsProvider.ISettings,
		IWWWRedirectSettings
	{
		private readonly ILogger<AppSettings> _logger;
		private readonly IConfiguration _config;

		public AppSettings(ILogger<AppSettings> logger, IConfiguration config)
		{
			_logger = logger;
			_config = config;
		}

		public string KeyVaultUrl => _config[nameof(KeyVaultUrl)];

        public string TechQuizUrl => _config[nameof(TechQuizUrl)];

		public int SecretCacheTimeoutMinutes => GetValue(nameof(SecretCacheTimeoutMinutes), 60);

		public AzureAdB2CSettings AzureAdB2C
		{
			get
			{
				var b2cSettings = new AzureAdB2CSettings();
				_config.Bind("AzureAdB2C", b2cSettings);
				return b2cSettings;
			}
		}

		/// <summary>
		/// Returns the <see cref="string"/> value of the appsetting.
		/// </summary>
		/// <param name="key">Name of app setting</param>
		/// <returns><see cref="string"/> value of the appsetting <paramref name="key"/></returns>
		/// <exception cref="MissingAppSettingException">If the <paramref name="key"/> is null/not found in the configuration!</exception>
		private string GetValue(string key)
		{
			string value = _config[key];

			if (value == null)
			{
				throw new MissingAppSettingException(key);
			}

			return value;
		}

		/// <summary>
		/// Returns the <see cref="string"/> value of the appsetting.
		/// </summary>
		/// <param name="key">Name of app setting</param>
		/// <param name="defaultValue">The default <see cref="string"/> value to return if the key is null/not found in the configuration.</param>
		/// <returns><see cref="string"/> value of the appsetting <paramref name="key"/>. If the key is null/not found then the default <see cref="string"/> value is returned.</returns>
		private string GetValue(string key, string defaultValue)
		{
			return _config[key] ?? defaultValue;
		}

		/// <summary>
		/// Returns the <typeparamref name="T"/> value of the appsetting.
		/// </summary>
		/// <typeparam name="T">Expected type of the appsetting. E.g. int, bool, double.</typeparam>
		/// <param name="key">Name of app setting</param>
		/// <returns><typeparamref name="T"/> value of the appsetting <paramref name="key"/></returns>
		/// <exception cref="MissingAppSettingException">If the <paramref name="key"/> is null/not found in the configuration!</exception>
		private T GetValue<T>(string key)
		{
			T value = GetValue(key, default(T));

			if (value == null)
			{
				throw new MissingAppSettingException(key);
			}

			return value;
		}

		/// <summary>
		/// Returns the <typeparamref name="T"/> value of the appsetting.
		/// </summary>
		/// <param name="key">Name of app setting</param>
		/// <param name="defaultValue">The default <typeparamref name="T"/> value to return if the key is null/not found in the configuration.</param>
		/// <typeparam name="T">Expected type of the appsetting. E.g. int, bool, double.</typeparam>
		/// <returns><typeparamref name="T"/> value of the appsetting <paramref name="key"/>. If the key is null/not found then the default <see cref="string"/> value is returned.</returns>
		private T GetValue<T>(string key, T defaultValue)
		{
			string value = _config[key];
			if (value == null)
			{
				return defaultValue;
			}

			try
			{
				return (T)Convert.ChangeType(value, typeof(T));
			}
			catch (Exception ex) when (ex is InvalidCastException || ex is OverflowException || ex is FormatException)
			{
				_logger.LogError(ex, "Could not convert app setting {key} to the required {type}", key, typeof(T));
				throw;
			}
		}
	}
}
