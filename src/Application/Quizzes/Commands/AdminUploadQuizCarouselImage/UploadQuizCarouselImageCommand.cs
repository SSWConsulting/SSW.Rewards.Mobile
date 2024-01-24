namespace Microsoft.Extensions.DependencyInjection.Quizzes.Commands.AdminUploadQuizCarouselImage;

public class UploadQuizCarouselImageCommand : IRequest<string>
{
    public int Id { get; set; }
    public Stream File { get; set; }
}

public class UploadQuizCarouselImageHandler : IRequestHandler<UploadQuizCarouselImageCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IQuizImageStorageProvider _storage;
    public ICurrentUserService _currentUserService { get; }

    public UploadQuizCarouselImageHandler(ICurrentUserService currentUserService,
        IApplicationDbContext ApplicationDbContext,
        IQuizImageStorageProvider quizImageStorageProvider)
    {
        _currentUserService = currentUserService;
        _storage = quizImageStorageProvider;
        _context = ApplicationDbContext;
    }

    public async Task<string> Handle(UploadQuizCarouselImageCommand request, CancellationToken cancellationToken)
    {
        await using var ms = new MemoryStream();
        var file = request.File;
        await file.CopyToAsync(ms, cancellationToken);

        byte[] bytes = ms.ToArray();

        string filename = Guid.NewGuid().ToString();

        var imgUrl = await _storage.UploadCarouselImage(bytes, filename);

        var quiz = await _context.Quizzes.FirstOrDefaultAsync(x => x.Id == request.Id);

        quiz.CarouselPhoto = imgUrl;
        await _context.SaveChangesAsync(cancellationToken);

        return imgUrl;
    }
}
