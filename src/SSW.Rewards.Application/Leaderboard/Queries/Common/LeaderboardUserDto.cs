using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Leaderboard.Queries.Common;
public class LeaderboardUserDto
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProfilePic { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int Balance { get; set; }
    public int PointsThisMonth { get; set; }
    public int PointsThisYear { get; set; }
}
