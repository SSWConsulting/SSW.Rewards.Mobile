using MediatR;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Reward.Queries.Common;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Reward.Commands.AddReward
{
    public class AddRewardCommand : IRequest<int>
    {
        public RewardViewModel reward { get; set; }
        public byte[] ImageBytes { get; set; }
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
            var imageUri = await _picStorageProvider.UploadRewardPic(request.ImageBytes, request.ImageFileName);

            var codeData = Encoding.ASCII.GetBytes(request.reward.Name);
            string code = Convert.ToBase64String(codeData);

            var entity = new SSW.Rewards.Domain.Entities.Reward
            {
                Name = request.reward.Name,
                Cost = request.reward.Cost,
                RewardType = request.reward.RewardType,
                ImageUri = imageUri.AbsoluteUri,
                Code = code
            };

            _context.Rewards.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }

}
