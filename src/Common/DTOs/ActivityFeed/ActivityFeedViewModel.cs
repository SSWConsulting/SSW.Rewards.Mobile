namespace SSW.Rewards.Shared.DTOs.ActivityFeed;

public class ActivityFeedViewModel
{
    public IEnumerable<ActivityFeedItemDto> Feed { get; set; } = Enumerable.Empty<ActivityFeedItemDto>();
}