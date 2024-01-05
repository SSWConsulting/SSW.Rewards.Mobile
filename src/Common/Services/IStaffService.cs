using SSW.Rewards.Shared.DTOs.Staff;

namespace SSW.Rewards.Shared.Services;

public interface IStaffService
{
    Task<StaffListViewModel> GetStaffList(CancellationToken cancellationToken);

    Task<StaffMemberDto> GetStaffMember(int id, CancellationToken cancellationToken);

    Task<StaffMemberDto> SearchStaffMember(StaffMemberQueryDto query, CancellationToken cancellationToken);
}
