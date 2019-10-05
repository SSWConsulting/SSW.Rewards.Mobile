using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SSW.Consulting.Models;
using System.Net.Http.Headers;
using System.Collections.ObjectModel;

namespace SSW.Consulting.Services
{
    public class LeaderService : ILeaderService
    {
        private LeaderboardClient _leaderBoardClient;
        private HttpClient _httpClient;
        private IUserService _userService;

        public LeaderService(IUserService userService)
        {
            _userService = userService;
            _httpClient = new HttpClient();
            Initialise();
        }

        private async Task Initialise()
        {
            string token = await _userService.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _leaderBoardClient = new LeaderboardClient("https://sswconsulting-dev.azurewebsites.net", _httpClient);
        }

        public async Task<IEnumerable<LeaderSummary>> GetLeadersAsync(bool forceRefresh)
        {
            var apiLeaderList = await _leaderBoardClient.GetAsync();

            List<LeaderSummary> summaries = new List<LeaderSummary>();

            foreach(var Leader in apiLeaderList.Users)
            {
                LeaderSummary leaderSummary = new LeaderSummary
                {
                    BaseScore = Leader.Points,
                    id = Leader.UserId,
                    Name = Leader.Name,
                    Rank = Leader.Rank,
                    ProfilePic = Leader.ProfilePic.ToString()
                };

                summaries.Add(leaderSummary);
            }

            return summaries;
        }
    }
}
