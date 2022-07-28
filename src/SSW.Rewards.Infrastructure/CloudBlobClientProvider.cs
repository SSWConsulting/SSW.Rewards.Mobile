using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace SSW.Rewards.Infrastructure
{
	public class CloudBlobClientProvider : ICloudBlobClientProvider
	{
		private readonly CloudBlobClient _client;

		public interface ISecrets
		{
			string ContentStorageConnectionString { get; }
		}

		public CloudBlobClientProvider(ISecrets secrets)
		{
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(secrets.ContentStorageConnectionString);
			_client = storageAccount.CreateCloudBlobClient();
		}

		public CloudBlobClient GetClient()
		{
			return _client;
		}
	}

	public interface ICloudBlobClientProvider
	{
		CloudBlobClient GetClient();
	}
}
