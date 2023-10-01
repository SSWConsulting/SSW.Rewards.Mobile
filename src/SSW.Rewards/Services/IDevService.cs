namespace SSW.Rewards.Services;

public interface IDevService
{
    Task<IEnumerable<DevProfile>> GetProfilesAsync();
}
