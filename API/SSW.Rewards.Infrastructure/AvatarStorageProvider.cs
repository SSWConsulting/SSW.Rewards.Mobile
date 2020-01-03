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

        private const string CONTAINER_NAME = "profile";

        public AvatarStorageProvider(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task<string> UploadAvatar(IFormFile file)
        {
            var ms = new MemoryStream();
            file.CopyTo(ms);
            
            Guid id = Guid.NewGuid();
            await _storageProvider.UploadBlob("avatars", id.ToString(), ms.ToArray());

            return id.ToString();
        }
    }
}
