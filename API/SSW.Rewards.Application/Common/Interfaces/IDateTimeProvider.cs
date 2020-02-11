using System;

namespace SSW.Rewards.Application.Common.Interfaces
{
	public interface IDateTimeProvider
	{
		DateTime Now { get; }
		DateTime UtcNow { get; }
	}
}