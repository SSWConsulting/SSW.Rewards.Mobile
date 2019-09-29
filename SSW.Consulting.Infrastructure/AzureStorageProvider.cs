using Microsoft.Azure.Storage.Blob;
using SSW.Consulting.Application.Exceptions;
using System.IO;
using System.Threading.Tasks;
using SSW.Consulting.Application.Interfaces;

namespace SSW.Consulting.Infrastructure
{
	public class AzureStorageProvider : IStorageProvider
	{
		private readonly CloudBlobClient _client;

		public AzureStorageProvider(ICloudBlobClientProvider clientProvider)
		{
			_client = clientProvider.GetClient();
		}

		public async Task UploadBlob(string containerName, string filename, byte[] contents)
		{
			CloudBlobContainer container = _client.GetContainerReference(containerName.ToLower());
			await container.CreateIfNotExistsAsync();

			CloudBlockBlob blob = container.GetBlockBlobReference(filename);
			await blob.UploadFromByteArrayAsync(contents, 0, contents.Length);
		}

		public async Task<byte[]> DownloadBlob(string containerName, string blobName)
		{
			CloudBlobContainer container = _client.GetContainerReference(containerName.ToLower());
			if (!container.Exists())
			{
				throw new BlobContainerNotFoundException(containerName);
			}

			CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
			if (!blob.Exists())
			{
				throw new BlobNotFoundException(containerName, blobName);
			}

			using (var ms = new MemoryStream())
			{
				await blob.DownloadToStreamAsync(ms);
				return ms.ToArray();
			}
		}
	}


}
