using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SSW.Consulting.Models;
using System.Net.Http.Headers;
using System.Collections.ObjectModel;
using Microsoft.AppCenter.Auth;
using Xamarin.Forms;

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
            _ = Initialise();
        }

        private async Task Initialise()
        {
            string token = await _userService.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string baseUrl = Constants.ApiBaseUrl;

            _leaderBoardClient = new LeaderboardClient(baseUrl, _httpClient);
        }

        public async Task<IEnumerable<LeaderSummary>> GetLeadersAsync(bool forceRefresh)
        {
            List<LeaderSummary> summaries = new List<LeaderSummary>();

            try
            {
                var apiLeaderList = await _leaderBoardClient.GetAsync();

                foreach (var Leader in apiLeaderList.Users)
                {
                    LeaderSummary leaderSummary = new LeaderSummary
                    {
                        BaseScore = Leader.Points,
                        id = Leader.UserId,
                        Name = Leader.Name,
                        Rank = Leader.Rank,
                        ProfilePic = string.IsNullOrWhiteSpace(Leader.ProfilePic?.ToString()) ? "icon_avatar" : Leader.ProfilePic.ToString()
                    };

                    summaries.Add(leaderSummary);
                }
            }
            catch(ApiException e)
            {
                //Console.Write(e);
                if(e.StatusCode == 401)
                {
                    await App.Current.MainPage.DisplayAlert("Authentication Failure", "Looks like your session has expired. Choose OK to go back to the login screen.", "OK");
                    Application.Current.MainPage = new SSW.Consulting.Views.LoginPage();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem loading the leaderboard. Please try again soon.", "OK");
                }
            }

            return summaries;
        }
    }
}
