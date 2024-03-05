using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.Services;

public interface IDevService
{
    Task<IEnumerable<NetworkingProfileDto>> GetProfilesAsync();
    
    Task<DevProfile> GetProfileAsync(string email);
}
