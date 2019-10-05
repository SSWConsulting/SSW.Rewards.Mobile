using System.Threading.Tasks;
using System;
using SSW.Consulting.Application.Common.Interfaces;

namespace SSW.Consulting.Infrastructure
{
    public class ProfileStorageProvider : IProfileStorageProvider
    {
        private readonly IStorageProvider _storageProvider;

        private const string CONTAINER_NAME = "profile";

        public ProfileStorageProvider(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task<Uri> GetProfileUri(string staffMemberName) => await _storageProvider.GetUri(CONTAINER_NAME, $"{staffMemberName.ToLower()}.png");

    }
}
