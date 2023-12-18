namespace SSW.Rewards.Application.Staff.Commands.UploadStaffMemberProfilePicture;

public class UploadStaffMemberProfilePictureCommand : IRequest<string>
{
    public int Id { get; set; }
    public Stream File { get; set; }
}

public class UploadProfilePicHandler : IRequestHandler<UploadStaffMemberProfilePictureCommand, string>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;
    private readonly IProfileStorageProvider _storage;
    public ICurrentUserService _currentUserService { get; }

    public UploadProfilePicHandler(ICurrentUserService currentUserService,
        IApplicationDbContext ApplicationDbContext,
        IProfileStorageProvider profileStorageProvider,
        IMapper mapper)
    {
        _currentUserService = currentUserService;
        _mapper = mapper;
        _storage = profileStorageProvider;
        _context = ApplicationDbContext;
    }

    public async Task<string> Handle(UploadStaffMemberProfilePictureCommand request, CancellationToken cancellationToken)
    {
        await using var ms = new MemoryStream();
        var file = request.File;
        await file.CopyToAsync(ms, cancellationToken);

        byte[] bytes = ms.ToArray();

        string filename = Guid.NewGuid().ToString();

        var imgUrl = await _storage.UploadProfilePicture(bytes, filename);

        var staffMember = await _context.StaffMembers.FirstOrDefaultAsync(x => x.Id == request.Id);

        staffMember.ProfilePhoto = imgUrl;
        await _context.SaveChangesAsync(cancellationToken);

        return imgUrl;
    }
}
