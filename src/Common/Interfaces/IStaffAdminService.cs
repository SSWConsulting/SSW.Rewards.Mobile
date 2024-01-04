using Shared.DTOs.Staff;

namespace Shared.Interfaces;

public interface IStaffAdminService
{
    Task DeleteStaffMember(int id, CancellationToken cancellationToken);

    Task UploadProfilePicture(int id, Stream file, CancellationToken cancellationToken);

    Task<StaffMemberDto> UpsertStaffMemberProfile(StaffMemberDto dto, CancellationToken cancellationToken);
}
