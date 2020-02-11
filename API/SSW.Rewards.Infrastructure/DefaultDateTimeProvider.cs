using SSW.Rewards.Application.Common.Interfaces;
using System;

namespace SSW.Rewards.Infrastructure
{
	public class DefaultDateTimeProvider : IDateTimeProvider
	{
		public DateTime Now => DateTime.Now;
		public DateTime UtcNow => DateTime.UtcNow;
	}
}