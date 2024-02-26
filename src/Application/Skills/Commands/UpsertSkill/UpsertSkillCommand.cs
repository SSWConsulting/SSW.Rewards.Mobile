namespace SSW.Rewards.Application.Skills.Commands.UpsertSkill;

public class UpsertSkillCommand : IRequest<int>
{
    public int Id { get; set; }
    public string Skill { get; set; }
    public string? ImageBytesInBase64 { get; set; }
    public string? ImageFileName { get; set; }
}

public class UpsertSkillCommandHandler : IRequestHandler<UpsertSkillCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly ISkillPicStorageProvider _picStorageProvider;

    public UpsertSkillCommandHandler(IApplicationDbContext context, 
        IDateTime dateTime,
        ISkillPicStorageProvider picStorageProvider)
    {
        _context = context;
        _dateTime = dateTime;
        _picStorageProvider = picStorageProvider;
    }

    public async Task<int> Handle(UpsertSkillCommand request, CancellationToken cancellationToken)
    {
        Uri imageUri = null;
        if (!string.IsNullOrWhiteSpace(request.ImageBytesInBase64))
        {
            var imageBytes = Convert.FromBase64String(request.ImageBytesInBase64);
            imageUri = await _picStorageProvider.UploadSkillPic(imageBytes, request.ImageFileName);
        }
        
        var found = _context.Skills.FirstOrDefault(x => x.Id == request.Id);

        if (found == null)
        {
            found = new Skill
            {
                Name = request.Skill,
                ImageUri = imageUri?.AbsoluteUri,
                CreatedUtc = _dateTime.Now
            };

            _context.Skills.Add(found);
        }
        else
        {
            found.Name = request.Skill;
            found.ImageUri = imageUri?.AbsoluteUri;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return found.Id;
    }
}
