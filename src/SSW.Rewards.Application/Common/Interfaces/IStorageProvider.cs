namespace SSW.Rewards.Application.Common.Interfaces;
public interface IStorageProvider
{
    Task<Uri> GetUri(string containerName, string blobName);
    Task<bool> Exists(string containerName, string blobName);
    Task UploadBlob(string containerName, string filename, byte[] contents);
    Task<byte[]> DownloadBlob(string containerName, string blobName);
}
