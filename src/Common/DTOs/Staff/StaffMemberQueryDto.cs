namespace Shared.DTOs.Staff;
public class StaffMemberQueryDto
{
    public int? Id { get; set; }

    public string? email { get; set; }

    public bool GetByEmail { get; set; } = false;
}
