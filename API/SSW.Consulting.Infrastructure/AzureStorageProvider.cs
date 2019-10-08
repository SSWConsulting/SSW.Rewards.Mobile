using Microsoft.Azure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;
using System;
using SSW.Consulting.Application.Common.Exceptions;
using SSW.Consulting.Application.Common.Interfaces;

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
            var container = await GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(filename);
            await blob.UploadFromByteArrayAsync(contents, 0, contents.Length);
        }

        public async Task<byte[]> DownloadBlob(string containerName, string blobName)
        {
            var container = await GetContainerReference(containerName);

            var blob = container.GetBlockBlobReference(blobName);
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

        public async Task<Uri> GetUri(string containerName, string blobName)
        {
            var container = await GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(blobName);
            return await blob.ExistsAsync() ? blob.Uri : null;
        }

        public async Task<bool> Exists(string containerName, string blobName)
        {
            var container = await GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(blobName);
            return await blob.ExistsAsync();
        }

        private async Task<CloudBlobContainer> GetContainerReference(string containerName)
        {
            var container = _client.GetContainerReference(containerName.ToLower());
            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            return container;
        }
    }


}
