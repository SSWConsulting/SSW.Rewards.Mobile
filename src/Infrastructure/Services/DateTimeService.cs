using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Infrastructure.Services;
public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}