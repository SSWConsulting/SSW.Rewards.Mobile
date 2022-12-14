using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Rewards.Models
{
	public class ExternalReward
	{
		public ExternalReward()
		{
		}

		public string Title { get; set; }
		public string Badge { get; set; }
		public int Points { get; set; }
		public string Picture { get; set; }
		public bool IsBonus { get; set; }
		public string Url { get; set; }
	}
}
