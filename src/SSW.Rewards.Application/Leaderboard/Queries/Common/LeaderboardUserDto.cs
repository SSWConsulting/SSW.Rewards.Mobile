using System;

namespace SSW.Rewards.Application.Leaderboard.Queries.Common
{
    public class LeaderboardUserDto
    {
        public int Rank { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; }

        public string ProfilePic { get; set; }

        public int TotalPoints { get; set; }

        public int Balance { get; set; }

        public int PointsThisMonth { get; set; }

        public int PointsThisYear { get; set; }
    }
}
