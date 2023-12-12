using Azure.Storage.Blobs;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Infrastructure;

public class AzureStorageProvider : IStorageProvider
{
    private readonly BlobServiceClient _client;

    public AzureStorageProvider(BlobServiceClient client)
    {
        _client = client;
    }

    public async Task UploadBlob(string containerName, string filename, byte[] contents)
    {
        var container = _client.GetBlobContainerClient(containerName);

        if (await container.ExistsAsync())
        {
            var blob = container.GetBlobClient(filename);

            await blob.UploadAsync(new MemoryStream(contents));
        }
        else
        {
            throw new BlobNotFoundException(containerName, filename);
        }
    }

    public async Task<byte[]> DownloadBlob(string containerName, string blobName)
    {
        var container = _client.GetBlobContainerClient(containerName);

        var blob = container.GetBlobClient(blobName);
        if (!blob.Exists())
        {
            throw new BlobNotFoundException(containerName, blobName);
        }

        using (var ms = new MemoryStream())
        {
            await blob.DownloadToAsync(ms);
            return ms.ToArray();
        }
    }

    public async Task<Uri?> GetUri(string containerName, string blobName)
    {
        var container = _client.GetBlobContainerClient(containerName);
        var blob = container.GetBlobClient(blobName);
        return await blob.ExistsAsync() ? blob.Uri : null;
    }

}


