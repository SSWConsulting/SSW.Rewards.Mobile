using MediatR;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Rewards.Queries.Common;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Rewards.Commands.AddReward
{
    public class AddRewardCommand : RewardViewModel, IRequest<int>
    {
        public string ImageBytesInBase64 { get; set; }
        public string ImageFileName { get; set; }
    }

    public class AddRewardCommandHandler : IRequestHandler<AddRewardCommand, int>
    {
        private readonly ISSWRewardsDbContext _context;
        private readonly IRewardPicStorageProvider _picStorageProvider;

        public AddRewardCommandHandler(ISSWRewardsDbContext context, IRewardPicStorageProvider picStorageProvider)
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



            var codeData = Encoding.ASCII.GetBytes(request.Name);
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

}
