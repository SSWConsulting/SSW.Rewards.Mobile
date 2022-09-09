using System.Text;
using SSW.Rewards.Application.Rewards.Common;

namespace SSW.Rewards.Application.Rewards.Commands.AddReward;

public class AddRewardCommand : IRequest<int>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Cost { get; set; }
    public string ImageUri { get; set; }
    public RewardType RewardType { get; set; }
    public string ImageBytesInBase64 { get; set; }
    public string ImageFileName { get; set; }
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
            Name = request.Name,
            Cost = request.Cost,
            RewardType = request.RewardType,
            ImageUri = imageUri?.AbsoluteUri,
            Code = code
        };

        _context.Rewards.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

