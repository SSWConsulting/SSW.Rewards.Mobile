using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SSW.Consulting.Models;
using SSW.Consulting.Helpers;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.ObjectModel;

namespace SSW.Consulting.Services
{
    public class ChallengeService : IChallengeService
    {
        private AchievementClient _achievementClient { get; set; }
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
            var myChallenges = new List<MyChallenge>
            {
                new MyChallenge { id = 1, Badge = "link", IsBonus = false, Points = 10, Title="Ulysses Maclaren", Picture = "points_twitter", Completed = true},
                new MyChallenge { id = 2, Badge = "link", IsBonus = true, Points = 10, Title="A. Cogan's Chinafy Talk", Picture = "points_quiz", Completed = true},
                new MyChallenge { id = 3, Badge = "external", IsBonus = false, Points = 10, Title="Matt Wicks", Picture = "points_youtube", Completed = true},
                new MyChallenge { id = 4, Badge = "link", IsBonus = false, Points = 10, Title="Penny Walker", Picture = "points_presentations", Completed = false},
                new MyChallenge { id = 5, Badge = "link", IsBonus = true, Points = 10, Title="SSW TV", Picture = "points_youtube", Completed = false},
                new MyChallenge { id = 6, Badge = "link", IsBonus = false, Points = 10, Title="Tech Quiz", Picture = "points_presentations", Completed = false},
                new MyChallenge { id = 7, Badge = "link", IsBonus = false, Points = 10, Title="Penny Walker", Picture = "points_presentations", Completed = false},
                new MyChallenge { id = 8, Badge = "link", IsBonus = false, Points = 10, Title="SSW TV", Picture = "points_youtube", Completed = false},
                new MyChallenge { id = 9, Badge = "link", IsBonus = false, Points = 10, Title="Tech Quiz", Picture = "points_presentations", Completed = false},
                new MyChallenge { id = 10, Badge = "link", IsBonus = false, Points = 10, Title="Penny Walker", Picture = "points_presentations", Completed = false},
                new MyChallenge { id = 11, Badge = "link", IsBonus = false, Points = 10, Title="SSW TV", Picture = "points_youtube", Completed = false},
                new MyChallenge { id = 12, Badge = "link", IsBonus = false, Points = 10, Title="Tech Quiz", Picture = "points_presentations", Completed = false},
                new MyChallenge { id = 1, Badge = "link", IsBonus = false, Points = 10, Title="Ulysses Maclaren", Picture = "points_twitter", Completed = true},
                new MyChallenge { id = 2, Badge = "link", IsBonus = true, Points = 10, Title="A. Cogan's Chinafy Talk", Picture = "points_quiz", Completed = true},
                new MyChallenge { id = 3, Badge = "external", IsBonus = false, Points = 10, Title="Matt Wicks", Picture = "points_youtube", Completed = true},
                new MyChallenge { id = 4, Badge = "link", IsBonus = false, Points = 10, Title="Penny Walker", Picture = "points_presentations", Completed = false},
                new MyChallenge { id = 5, Badge = "link", IsBonus = true, Points = 10, Title="SSW TV", Picture = "points_youtube", Completed = false},
                new MyChallenge { id = 6, Badge = "link", IsBonus = false, Points = 10, Title="Tech Quiz", Picture = "points_presentations", Completed = false},
                new MyChallenge { id = 7, Badge = "link", IsBonus = false, Points = 10, Title="Penny Walker", Picture = "points_presentations", Completed = false},
                new MyChallenge { id = 8, Badge = "link", IsBonus = false, Points = 10, Title="SSW TV", Picture = "points_youtube", Completed = false},
                new MyChallenge { id = 9, Badge = "link", IsBonus = false, Points = 10, Title="Tech Quiz", Picture = "points_presentations", Completed = false},
                new MyChallenge { id = 10, Badge = "link", IsBonus = false, Points = 10, Title="Penny Walker", Picture = "points_presentations", Completed = false},
                new MyChallenge { id = 11, Badge = "link", IsBonus = false, Points = 10, Title="SSW TV", Picture = "points_youtube", Completed = false},
                new MyChallenge { id = 12, Badge = "link", IsBonus = false, Points = 10, Title="Tech Quiz", Picture = "points_presentations", Completed = false}

            };

            foreach (var challenge in myChallenges)
            {
                _myChallenges.Add(challenge);
            }

            return await Task.FromResult(_myChallenges);
        }

        public async Task<ChallengeResult> PostChallengeAsync(string achievementString)
        {
            try
            {
                AchievementViewModel response = await _achievementClient.AddAsync(achievementString);

                if (response != null)
                {
                    return ChallengeResult.Added;
                }
                else if (response.Id == 206)
                {
                    return ChallengeResult.Duplicate;
                }
                else
                {
                    return ChallengeResult.NotFound;
                }
            }
            catch
            {
                return ChallengeResult.NotFound;
            }
        }
    }
}
