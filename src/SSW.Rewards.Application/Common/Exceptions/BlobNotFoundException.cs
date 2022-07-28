namespace SSW.Rewards.Application.Common.Exceptions;
public class BlobNotFoundException : Exception
{
    public string ContainerName { get; } = string.Empty;
    public string BlobName { get; } = string.Empty;
    public BlobNotFoundException()
    {
    }

    public BlobNotFoundException(string containerName, string blobName) : base($"")
    {
        ContainerName = containerName;
        BlobName = blobName;
    }
}
