

namespace SSW.Rewards.Shared.DTOs.Staff;
public class StaffSkillDto
{
    public required string Name { get; set; }

    public string ImageUri { get; set; } = string.Empty;
    
    public SkillLevel Level { get; set; }
}
