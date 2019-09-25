using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Consulting.Application.Leaderboard.Queries.GetLeaderboardList
{
    public class GetLeaderboardListQuery : IRequest<LeaderboardListViewModel>
    {
    }
}
