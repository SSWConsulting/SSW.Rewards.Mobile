using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Consulting.Application.Leaderboard.Queries.GetLeaderboardList
{
    public class LeaderboardUserDto
    {      
        public int Position { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public int Bonus { get; set; }
    }
}
