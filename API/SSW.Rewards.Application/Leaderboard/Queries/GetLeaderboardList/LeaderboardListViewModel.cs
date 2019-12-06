using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList
{
    public class LeaderboardListViewModel
    {
        public IEnumerable<LeaderboardUserDto> Users { get; set; }

    }
}
