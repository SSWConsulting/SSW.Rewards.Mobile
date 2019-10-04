using System;

namespace SSW.Consulting.Application.Common.Exceptions
{
    public class BlobContainerNotFoundException : Exception
    {
        public BlobContainerNotFoundException()
        {
        }

        public BlobContainerNotFoundException(string containerName) : base($"The blob container '{containerName}' does not exist in the storage account.")
        {
            ContainerName = containerName;
        }

        public string ContainerName { get; }
    }
}
