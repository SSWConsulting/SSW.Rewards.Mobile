using System.Diagnostics;

namespace SSW.Rewards.Shared.DTOs.Users;

[DebuggerDisplay("Id = {Id}, Rank = {Rank}, Points = {Points}")]
public class UserRankingMinimumDto
{
    public UserRankingMinimumDto() { }

    public UserRankingMinimumDto(int id, int rank, int points)
    {
        Id = id;
        Rank = rank;
        Points = points;
    }

    public int Id { get; set; }
    public int Rank { get; set; }
    public int Points { get; set; }
}
