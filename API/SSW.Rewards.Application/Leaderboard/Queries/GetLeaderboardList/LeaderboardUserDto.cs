using System;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList
{
    public class LeaderboardUserDto
    {      
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public Uri ProfilePic { get; set; }
        public int Points { get; set; }
    }
}
