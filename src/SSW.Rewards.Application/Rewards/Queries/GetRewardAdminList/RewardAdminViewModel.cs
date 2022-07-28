using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Rewards.Application.Rewards.Queries.GetRewardAdminList
{
    public class RewardAdminViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public string Code { get; set; }
    }
}
