using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Shared.DTOs.Network;

public class NetworkProfileListViewModel
{
    public IEnumerable<NetworkProfileDto> Profiles { get; set; } = Enumerable.Empty<NetworkProfileDto>();
}