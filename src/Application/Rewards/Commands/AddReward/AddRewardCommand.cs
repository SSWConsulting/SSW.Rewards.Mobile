using System.Text;

namespace SSW.Rewards.Application.Rewards.Commands.AddReward;

public class AddRewardCommand : IRequest<int>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Cost { get; set; }
    public string ImageUri { get; set; }
    public RewardType RewardType { get; set; }
    public string ImageBytesInBase64 { get; set; }
    public string ImageFileName { get; set; }
    public string CarouselImageBytesInBase64 { get; set; }
    public string CarouselImageFileName { get; set; }
    public bool IsCarousel { get; set; }
}

public class AddRewardCommandHandler : IRequestHandler<AddRewardCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IRewardPicStorageProvider _picStorageProvider;

    public AddRewardCommandHandler(IApplicationDbContext context, IRewardPicStorageProvider picStorageProvider)
    {
        _context = context;
        _picStorageProvider = picStorageProvider;
    }

    public async Task<int> Handle(AddRewardCommand request, CancellationToken cancellationToken)
    {
        Uri imageUri = await UploadImage(request.ImageBytesInBase64, request.ImageFileName);
        Uri carouselImageUri = await UploadImage(request.CarouselImageBytesInBase64, request.CarouselImageFileName);

        var codeData = Encoding.ASCII.GetBytes($"rwd:{request.Name}");
        string code = Convert.ToBase64String(codeData);

        var entity = new Reward
        {
            Name = request.Name,
            Cost = request.Cost,
            Description = request.Description,
            RewardType = request.RewardType,
            ImageUri = imageUri?.AbsoluteUri,
            CarouselImageUri = carouselImageUri?.AbsoluteUri,
            IsCarousel = request.IsCarousel,
            Code = code
        };

        _context.Rewards.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
    
    private async Task<Uri> UploadImage(string imageBytesInBase64, string imageFileName)
    {
        if (string.IsNullOrWhiteSpace(imageBytesInBase64))
        {
            return null;
        }

        var imageBytes = Convert.FromBase64String(imageBytesInBase64);
        return await _picStorageProvider.UploadRewardPic(imageBytes, imageFileName);
    }
}

