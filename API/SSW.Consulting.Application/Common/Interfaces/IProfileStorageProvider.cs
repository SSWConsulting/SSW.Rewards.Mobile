using System;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.Common.Interfaces
{
    public interface IProfileStorageProvider
    {
        Task<Uri> GetProfileUri(string staffMemberName);
    }
}
