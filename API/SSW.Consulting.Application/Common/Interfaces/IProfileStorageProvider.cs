using System;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.Common.Interfaces
{
    public interface IProfileStorageProvider
    {
        Task<byte[]> GetProfileData();
        Task<Uri> GetProfileUri(string staffMemberName);
    }
}
