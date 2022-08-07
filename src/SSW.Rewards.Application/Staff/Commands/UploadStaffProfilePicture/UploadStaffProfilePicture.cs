using Microsoft.AspNetCore.Http;

namespace SSW.Rewards.Application.Staff.Commands.UploadStaffProfilePicture;
public class UploadStaffProfilePicture : IRequest<string>
{
    public int Id { get; set; }
    public IFormFile File { get; set; }

    public UploadStaffProfilePicture(int id, IFormFile file)
    {
        Id      = id;
        File    = file;
    }
}

public class UploadStaffProfilePictureHandler : IRequestHandler<UploadStaffProfilePicture, string>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;
    private readonly IProfileStorageProvider _storage;
    public ICurrentUserService _currentUserService { get; }

    public UploadStaffProfilePictureHandler(
        ICurrentUserService currentUserService,
        IApplicationDbContext ApplicationDbContext,
        IProfileStorageProvider profileStorageProvider,
        IMapper mapper)
    {
        _currentUserService = currentUserService;
        _mapper = mapper;
        _storage = profileStorageProvider;
        _context = ApplicationDbContext;
    }

    public async Task<string> Handle(UploadStaffProfilePicture request, CancellationToken cancellationToken)
    {
        await using var ms = new MemoryStream();
        IFormFile file = request.File;
        await file.CopyToAsync(ms, cancellationToken);

        byte[] bytes = ms.ToArray();

        string filename = file.FileName;

        var imgUrl = await _storage.UploadProfilePicture(bytes, filename);

        var staffMember = await _context.StaffMembers.FirstOrDefaultAsync(x => x.Id == request.Id);

        staffMember.ProfilePhoto = imgUrl;
        await _context.SaveChangesAsync(cancellationToken);

        return imgUrl;
    }
}