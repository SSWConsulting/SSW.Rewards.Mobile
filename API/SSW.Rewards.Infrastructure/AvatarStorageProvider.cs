using System.Threading.Tasks;
using System;
using SSW.Rewards.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace SSW.Rewards.Infrastructure
{
    public class AvatarStorageProvider : IAvatarStorageProvider
    {
        private readonly IStorageProvider _storageProvider;

        private const string CONTAINER_NAME = "avatars";

        public AvatarStorageProvider(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task<string> UploadAvatar(IFormFile file)
        {
            var ms = new MemoryStream();
            file.CopyTo(ms);

            string id = Guid.NewGuid().ToString();
            await _storageProvider.UploadBlob(CONTAINER_NAME, id, ms.ToArray());
            var uri = await _storageProvider.GetUri(CONTAINER_NAME, id);

            return uri.AbsoluteUri;
        }
    }
}
