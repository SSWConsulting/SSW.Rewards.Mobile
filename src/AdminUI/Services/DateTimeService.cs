using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Admin.UI.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
