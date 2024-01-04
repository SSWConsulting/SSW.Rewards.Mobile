namespace Shared.DTOs.Staff;

public class StaffListViewModel
{
    public IEnumerable<StaffMemberDto> Staff { get; set; } = Enumerable.Empty<StaffMemberDto>();
}
