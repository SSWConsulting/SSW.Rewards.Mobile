using Shared.Models;

namespace Shared.DTOs.Staff;
public class StaffSkillDto
{
    public required string Name { get; set; }

    public SkillLevel Level { get; set; }
}
