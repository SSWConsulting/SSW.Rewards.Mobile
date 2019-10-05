using System;

namespace SSW.Consulting.Application.Common.Exceptions
{
    public class BlobNotFoundException : Exception
    {
        public BlobNotFoundException()
        {
        }

        public BlobNotFoundException(string containerName, string blobName) : base($"")
        {
            ContainerName = containerName;
            BlobName = blobName;
        }

        public string ContainerName { get; }
        public string BlobName { get; }
    }
}
