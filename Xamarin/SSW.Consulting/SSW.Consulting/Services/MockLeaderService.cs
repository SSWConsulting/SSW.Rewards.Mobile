using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSW.Consulting.Models;
using SSW.Consulting.ViewModels;

namespace SSW.Consulting.Services
{
    class MockLeaderService : ILeaderService
    {
        List<LeaderSummary> leaders;

        public MockLeaderService()
        {
            var mockLeaders = new List<LeaderSummary>
            {
                new LeaderSummary {id = 1, Name = "Tan Wuhan", BaseScore = 120, BonusScore = 35, ProfilePic = "Tan", Rank = 1 },
                new LeaderSummary {id = 2, Name = "Calum Simpson", BaseScore = 120, BonusScore = 35, ProfilePic = "Calum", Rank = 2 },
                new LeaderSummary {id = 3, Name = "Matt Wicks", BaseScore = 120, BonusScore = 35, ProfilePic = "MattW", Rank = 3 },
                new LeaderSummary {id = 4, Name = "Tatiana Gagelman", BaseScore = 120, BonusScore = 35, ProfilePic = "adam", Rank = 4 },
                new LeaderSummary {id = 5, Name = "Matt Goldman", BaseScore = 120, BonusScore = 35, ProfilePic = "MattG", Rank = 5 },
                new LeaderSummary {id = 6, Name = "Adam Cogan", BaseScore = 120, BonusScore = 35, ProfilePic = "adam", Rank = 6 },
                new LeaderSummary {id = 7, Name = "Ulyses Mclaren", BaseScore = 120, BonusScore = 35, ProfilePic = "Uly", Rank = 7 },
                new LeaderSummary {id = 8, Name = "Andreas Lengkeek", BaseScore = 120, BonusScore = 35, ProfilePic = "Andreas", Rank = 8 },
                new LeaderSummary {id = 9, Name = "Tatiana Gagelman", BaseScore = 120, BonusScore = 35, ProfilePic = "adam", Rank = 9 },
                new LeaderSummary {id = 10, Name = "Matt Goldman", BaseScore = 120, BonusScore = 35, ProfilePic = "MattG", Rank = 10 },
                new LeaderSummary {id = 11, Name = "Adam Cogan", BaseScore = 120, BonusScore = 35, ProfilePic = "adam", Rank = 11 },
                new LeaderSummary {id = 12, Name = "Ulyses Mclaren", BaseScore = 120, BonusScore = 35, ProfilePic = "Uly", Rank = 12 },
                new LeaderSummary {id = 13, Name = "Andreas Lengkeek", BaseScore = 120, BonusScore = 35, ProfilePic = "Andreas", Rank = 13 }
            };

            leaders = mockLeaders.OrderBy(l => l.Rank).ToList();
        }
        public async Task<IEnumerable<LeaderSummary>> GetLeadersAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(leaders);
        }
    }
}
