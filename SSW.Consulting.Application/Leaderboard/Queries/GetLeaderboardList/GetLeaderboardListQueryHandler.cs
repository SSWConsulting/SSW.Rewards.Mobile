using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.Leaderboard.Queries.GetLeaderboardList
{
    public class GetLeaderboardListQueryHandler : IRequestHandler<GetLeaderboardListQuery, LeaderboardListViewModel>
    {
        public GetLeaderboardListQueryHandler()
        {
        }

        public async Task<LeaderboardListViewModel> Handle(GetLeaderboardListQuery request, CancellationToken cancellationToken)
        {
            // TODO: get from Cosmos

            var user1 = new LeaderboardUserDto() { Position = 1, Name = "Tan Wuhan", ImageUrl = "", Points = 120, Bonus = 35 };
            var user2 = new LeaderboardUserDto() { Position = 2, Name = "Matt Wicks", ImageUrl = "", Points = 120, Bonus = 35 };
            var user3 = new LeaderboardUserDto() { Position = 3, Name = "Vladena Klimkova", ImageUrl = "", Points = 120, Bonus = 35 };
            var user4 = new LeaderboardUserDto() { Position = 4, Name = "Tatiana Gagelman", ImageUrl = "", Points = 120, Bonus = 35 };
            var user5 = new LeaderboardUserDto() { Position = 5, Name = "Tatiana Gagelman", ImageUrl = "", Points = 120, Bonus = 35 };
            var user6 = new LeaderboardUserDto() { Position = 6, Name = "Adam Cogan", ImageUrl = "", Points = 120, Bonus = 35 };
            var user7 = new LeaderboardUserDto() { Position = 7, Name = "Tatiana Gagelman", ImageUrl = "", Points = 120, Bonus = 35 };
            var user8 = new LeaderboardUserDto() { Position = 8, Name = "Greg Harris", ImageUrl = "", Points = 120, Bonus = 35 };

            var users = new List<LeaderboardUserDto> { user1, user2, user3, user4, user5, user6, user7, user8 };

            var model = new LeaderboardListViewModel
            {
                Users = users
            };
            
            return model;
        }
    }
}
