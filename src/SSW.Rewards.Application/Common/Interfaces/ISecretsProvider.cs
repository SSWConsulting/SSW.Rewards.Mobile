using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Common.Interfaces
{
    public interface ISecretsProvider
    {
        Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken);
        string GetSecret(string secretName);
    }
}
