using Shared.Models;

namespace Shared.DTOs.Rewards;

public class RewardDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Cost { get; set; }
    public string? ImageUri { get; set; }
    public RewardType RewardType { get; set; }
}
