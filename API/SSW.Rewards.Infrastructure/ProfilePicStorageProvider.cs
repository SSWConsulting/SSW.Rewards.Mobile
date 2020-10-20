using System.Threading.Tasks;
using System;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Infrastructure
{
    public class ProfilePicStorageProvider : IProfilePicStorageProvider
    {
        private readonly IStorageProvider _storageProvider;

        private const string CONTAINER_NAME = "ProfilePics";

        public ProfilePicStorageProvider(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task<string> UploadProfilePic(byte[] imageArray, string fileName)
        {
            string id = Guid.NewGuid().ToString() + fileName;
            await _storageProvider.UploadBlob(CONTAINER_NAME, id, imageArray);

            var uri = await _storageProvider.GetUri(CONTAINER_NAME, id);

            return uri.AbsoluteUri;
        }

        public async Task<Uri> GetProfilePicUri(string picId) => await _storageProvider.GetUri(CONTAINER_NAME, picId);
    }
}
