using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Threading.Tasks;

namespace SSW.Rewards.Infrastructure
{
    public class RewardPicStorageProvider : IRewardPicStorageProvider
    {
        private readonly IStorageProvider _storageProvider;

        private const string CONTAINER_NAME = "RewardPics";

        public RewardPicStorageProvider(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task<Uri> GetRewardPicUri(string picId) => await _storageProvider.GetUri(CONTAINER_NAME, picId);

        public async Task<Uri> UploadRewardPic(byte[] imageArray, string filename)
        {
            string id = Guid.NewGuid().ToString() + filename;
            await _storageProvider.UploadBlob(CONTAINER_NAME, id, imageArray);

            var uri = await _storageProvider.GetUri(CONTAINER_NAME, id);

            return uri;
        }
    }
}
