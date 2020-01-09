using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Common.Interfaces
{
    public interface IAvatarStorageProvider
    {
        Task<string> UploadAvatar(IFormFile file);
    }
}
