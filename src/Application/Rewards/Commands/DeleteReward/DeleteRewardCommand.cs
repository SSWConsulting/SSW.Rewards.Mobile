﻿using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Rewards.Commands.DeleteReward;

public class DeleteRewardCommand : IRequest<Unit>
{
    public int Id { get; set; }
}

public class DeleteRewardCommandHandler : IRequestHandler<DeleteRewardCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public DeleteRewardCommandHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Unit> Handle(DeleteRewardCommand request, CancellationToken cancellationToken)
    {
        var findReward = await _context?.Rewards?
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (findReward == null)
        {
            throw new NotFoundException(nameof(DeleteRewardCommand), request.Id);
        }

        _context.Rewards.Remove(findReward);
        await _context.SaveChangesAsync(cancellationToken);

        _cacheService.Remove(CacheTags.UpdatedOnlyRewards);

        return Unit.Value;

    }
}
