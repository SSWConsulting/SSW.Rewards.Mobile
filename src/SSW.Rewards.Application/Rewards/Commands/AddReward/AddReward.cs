using System.Text;
using SSW.Rewards.Application.Rewards.Common;

namespace SSW.Rewards.Application.Rewards.Commands.AddReward;
public class AddReward : RewardViewModel, IRequest<int>
{
    public string ImageBytesInBase64 { get; set; } = string.Empty;
    public string ImageFileName { get; set; } = string.Empty;

    public AddReward(string imageBytesInBase64, string imageFileName)
    {
        ImageBytesInBase64  = imageBytesInBase64;
        ImageFileName       = imageFileName;
    }
}

public class AddRewardHandler : IRequestHandler<AddReward, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IRewardPicStorageProvider _picStorageProvider;

    public AddRewardHandler(
        IApplicationDbContext context, 
        IRewardPicStorageProvider picStorageProvider)
    {
        _context            = context;
        _picStorageProvider = picStorageProvider;
    }

    public async Task<int> Handle(AddReward request, CancellationToken cancellationToken)
    {
        Uri imageUri = null;
        if (!string.IsNullOrWhiteSpace(request.ImageBytesInBase64))
        {
            var imageBytes = Convert.FromBase64String(request.ImageBytesInBase64);
            imageUri = await _picStorageProvider.UploadRewardPic(imageBytes, request.ImageFileName);
        }

        var codeData = Encoding.ASCII.GetBytes($"rwd:{request.Name}");
        string code = Convert.ToBase64String(codeData);

        var entity = new SSW.Rewards.Domain.Entities.Reward
        {
            Name        = request.Name,
            Cost        = request.Cost,
            RewardType  = request.RewardType,
            ImageUri    = imageUri?.AbsoluteUri,
            Code        = code
        };

        _context.Rewards.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
