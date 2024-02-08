using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}