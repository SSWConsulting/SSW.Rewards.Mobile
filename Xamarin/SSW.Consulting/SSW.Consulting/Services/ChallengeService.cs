using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SSW.Consulting.Models;
using SSW.Consulting.Helpers;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.ObjectModel;
using System.Linq;

namespace SSW.Consulting.Services
{
    public class ChallengeService : IChallengeService
    {
        private AchievementClient _achievementClient { get; set; }
        private UserClient _userClient { get; set; }
        private HttpClient _httpClient { get; set; }
        private IUserService _userService;
        private ObservableCollection<Challenge> _challenges { get; set; }
        private ObservableCollection<MyChallenge> _myChallenges { get; set; }

        public ChallengeService(IUserService userService)
        {
            _userService = userService;
            _httpClient = new HttpClient();
            _challenges = new ObservableCollection<Challenge>();
            _myChallenges = new ObservableCollection<MyChallenge>();
            Initialise();
        }

        private async Task Initialise()
        {
            string token = await _userService.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string baseUrl = Constants.ApiBaseUrl;

            _achievementClient = new AchievementClient(baseUrl, _httpClient);
        }

        public async Task<IEnumerable<Challenge>> GetChallengesAsync()
        {
            var challenges = new List<Challenge>
            {
                new Challenge { id = 1, Badge = "link", IsBonus = false, Points = 10, Title="Link your Twitter account", Picture = "points_twitter"},
                new Challenge { id = 2, Badge = "link", IsBonus = false, Points = 10, Title="Take SSW tech quiz", Picture = "points_quiz"},
                new Challenge { id = 3, Badge = "external", IsBonus = false, Points = 10, Title="Subscribe to SSW TV", Picture = "points_youtube"},
                new Challenge { id = 4, Badge = "link", IsBonus = false, Points = 10, Title="See SSW talks @NDC", Picture = "points_presentations"},
                new Challenge { id = 5, Badge = "link", IsBonus = false, Points = 10, Title="Watch our Welcome video", Picture = "points_youtube"},
                new Challenge { id = 6, Badge = "link", IsBonus = false, Points = 10, Title="Visit Adam's blog", Picture = "points_presentations"},
                new Challenge { id = 7, Badge = "link", IsBonus = false, Points = 10, Title="Register for an event", Picture = "points_presentations"},
                new Challenge { id = 8, Badge = "link", IsBonus = false, Points = 10, Title="Follow SSW on LinkedIn", Picture = "points_twitter"}
            };

            foreach (var challenge in challenges)
            {
                _challenges.Add(challenge);
            }

            return await Task.FromResult(_challenges);
        }

        public async Task<IEnumerable<MyChallenge>> GetMyChallengesAsync()
        {
            _userClient = new UserClient(Constants.ApiBaseUrl, _httpClient);

            var myChallenges = await _userClient.AchievementsAsync(await _userService.GetMyUserIdAsync());

            foreach (var challenge in myChallenges.UserAchievements)
            {
                _myChallenges.Add(new MyChallenge
                {
                    Badge = "link",
                    Completed = challenge.Complete,
                    Title = challenge.AchievementName,
                    Points = challenge.AchievementValue,
                    IsBonus = false
                });
            }

            return await Task.FromResult(_myChallenges.OrderBy(c => c.Title));
        }

        public async Task<ChallengeResult> PostChallengeAsync(string achievementString)
        {
            try
            {
                AchievementViewModel response = await _achievementClient.AddAsync(achievementString);

                if (response != null)
                    return ChallengeResult.Added;
                else
                    return ChallengeResult.NotFound;
            }
            catch
            {
                return ChallengeResult.NotFound;
            }
        }
    }
}
