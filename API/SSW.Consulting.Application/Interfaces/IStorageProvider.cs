using System.Threading.Tasks;

namespace SSW.Consulting.Application.Interfaces
{
	public interface IStorageProvider
	{
		Task UploadBlob(string containerName, string filename, byte[] contents);
		Task<byte[]> DownloadBlob(string containerName, string blobName);
	}
}
