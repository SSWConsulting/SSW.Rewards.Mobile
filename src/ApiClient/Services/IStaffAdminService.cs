using SSW.Rewards.Shared.DTOs.Staff;

namespace SSW.Rewards.ApiClient.Services;

public interface IStaffAdminService
{
    Task DeleteStaffMember(int id, CancellationToken cancellationToken);

    Task UploadProfilePicture(int id, Stream file, CancellationToken cancellationToken);

    Task<StaffMemberDto> UpsertStaffMemberProfile(StaffMemberDto dto, CancellationToken cancellationToken);
}
