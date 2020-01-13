using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Common.Interfaces
{
    public interface IProfilePicStorageProvider
    {
        Task<string> UploadProfilePic(IFormFile file);
        Task<Uri> GetProfilePicUri(string picId);
    }
}
