using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Options;

namespace SSW.Rewards.Infrastructure;

public class CloudBlobClientProvider : ICloudBlobClientProvider
{
	private readonly CloudBlobClient _client;

	public CloudBlobClientProvider(IOptions<CloudBlobProviderOptions> options)
	{
		CloudStorageAccount storageAccount = CloudStorageAccount.Parse(options.Value.ContentStorageConnectionString);
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

public class CloudBlobProviderOptions
{
	public string ContentStorageConnectionString { get; set; }
}
