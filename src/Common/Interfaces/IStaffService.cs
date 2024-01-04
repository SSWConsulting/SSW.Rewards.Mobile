using Shared.DTOs.Staff;

namespace Shared.Interfaces;

public interface IStaffService
{
    Task<StaffListViewModel> GetStaffList(CancellationToken cancellationToken);

    Task<StaffMemberDto> GetStaffMember(StaffMemberQueryDto query, CancellationToken cancellationToken);
}
