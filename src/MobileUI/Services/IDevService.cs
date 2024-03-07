using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.Services;

public interface IDevService
{
    Task<IEnumerable<NetworkProfileDto>> GetProfilesAsync();
    
    Task<DevProfile> GetProfileAsync(string email);
}
