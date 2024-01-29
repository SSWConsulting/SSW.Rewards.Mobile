namespace SSW.Rewards.Application.Quizzes.Commands.AdminUploadQuizImage;

public class AdminUploadQuizImageCommand : IRequest<string>
{
    public Stream File { get; set; }
}

public class AdminUploadQuizImageHandler : IRequestHandler<AdminUploadQuizImageCommand, string>
{
    private readonly IQuizImageStorageProvider _storage;

    public AdminUploadQuizImageHandler(IQuizImageStorageProvider storage)
    {
        _storage = storage;
    }

    public async Task<string> Handle(AdminUploadQuizImageCommand request, CancellationToken cancellationToken)
    {

        await using var ms = new MemoryStream();
        var file = request.File;
        await file.CopyToAsync(ms, cancellationToken);

        byte[] bytes = ms.ToArray();

        string filename = Guid.NewGuid().ToString();
        await _storage.UploadCarouselImage(bytes, filename);

        return filename;
    }
}
