using System.Threading.Tasks;

namespace SSW.Consulting.Application.Interfaces
{
	public interface ISecretsProvider
	{
		Task<string> GetSecretAsync(string secretName);
		string GetSecret(string secretName);
	}
}
