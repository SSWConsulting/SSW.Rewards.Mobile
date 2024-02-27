using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Rewards.Commands.UpdateReward;

public class UpdateRewardCommand : IRequest<Unit>
{
    public int Id { get; set; }
    public int Cost { get; set; }
    public string? RewardName { get; set; }
    public string? ImageFilename { get; set; }
    public string? ImageBytesInBase64 { get; set; }
    public bool? IsOnboardingReward { get; set; }
    public string? CarouselImageBytesInBase64 { get; set; }
    public string? CarouselImageFileName { get; set; }
    public bool IsCarousel { get; set; }
}

public class UpdateRewardCommandHandler : IRequestHandler<UpdateRewardCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IRewardPicStorageProvider _picStorageProvider;

    public UpdateRewardCommandHandler(IApplicationDbContext context , IRewardPicStorageProvider picStorageProvider)
    {
        _context = context;
        _picStorageProvider = picStorageProvider;
    }

    public async Task<Unit> Handle(UpdateRewardCommand request, CancellationToken cancellationToken)
    {
        var reward = await _context.Rewards
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (reward == null)
        {
            throw new NotFoundException(nameof(UpdateRewardCommand), request.Id);
        }

        if (!string.IsNullOrWhiteSpace(request.ImageBytesInBase64) && !string.IsNullOrWhiteSpace(request.ImageFilename))
        {
            Uri imageUri = await UploadImage(request.ImageBytesInBase64, request.ImageFilename);
            reward.ImageUri = imageUri?.AbsoluteUri;
        }        
        
        if (!string.IsNullOrWhiteSpace(request.CarouselImageBytesInBase64) && !string.IsNullOrWhiteSpace(request.CarouselImageFileName))
        {
            Uri imageUri = await UploadImage(request.CarouselImageBytesInBase64, request.CarouselImageFileName);
            reward.CarouselImageUri = imageUri?.AbsoluteUri;
        }

        if (!string.IsNullOrWhiteSpace(request.RewardName))
        {
            reward.Name = request.RewardName;
        }

        if (request.IsOnboardingReward is not null)
        {
            reward.IsOnboardingReward = request.IsOnboardingReward.Value;
        }

        reward.Cost = request.Cost;
        reward.IsCarousel = request.IsCarousel;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
    
    private async Task<Uri> UploadImage(string imageBytesInBase64, string imageFileName)
    {
        var imageBytes = Convert.FromBase64String(imageBytesInBase64);
        return await _picStorageProvider.UploadRewardPic(imageBytes, imageFileName);
    }
}
