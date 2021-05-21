using SSW.Rewards.Models;
using SSW.Rewards.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SSW.Rewards.Services
{
    public class ChallengeService : IChallengeService
    {
        private AchievementClient _achievementClient { get; set; }
        private RewardClient _rewardClient { get; set; }
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
            _ = Initialise();
        }

        private async Task Initialise()
        {
            string token = await _userService.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string baseUrl = App.Constants.ApiBaseUrl;

            _achievementClient = new AchievementClient(baseUrl, _httpClient);
            _rewardClient = new RewardClient(baseUrl, _httpClient);
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

        private async Task<ChallengeResultViewModel> PostChallengeAsync(string achievementString)
        {
            ChallengeResultViewModel vm = new ChallengeResultViewModel();

            try
            {
                PostAchievementResult response = await _achievementClient.PostAsync(achievementString);

                if (response != null)
                {
                    switch (response.Status)
                    {
                        case AchievementStatus.Added:
                            vm.result = ChallengeResult.Added;
                            vm.Title = response.ViewModel.Name;
                            vm.Points = response.ViewModel.Value;
                            break;

                        case AchievementStatus.Duplicate:
                            vm.result = ChallengeResult.Duplicate;
                            vm.Title = "Duplicate";
                            vm.Points = 0;
                            break;

                        case AchievementStatus.Error:
                            vm.result = ChallengeResult.Error;
                            vm.Title = "Error";
                            vm.Points = 0;
                            break;

                        case AchievementStatus.NotFound:
                            vm.result = ChallengeResult.NotFound;
                            vm.Title = "Unrecognised";
                            vm.Points = 0;
                            break;
                    }
                }
                else
                    vm.result = ChallengeResult.Error;
            }
            catch (ApiException e)
            {
                if (e.StatusCode == 401)
                {
                    await App.Current.MainPage.DisplayAlert("Authentication Failure", "Looks like your session has expired. Choose OK to go back to the login screen.", "OK");
                    Application.Current.MainPage = new SSW.Rewards.Views.LoginPage();
                }
                else
                {
                    vm.result = ChallengeResult.Error;
                }
            }
            catch
            {
                vm.result = ChallengeResult.Error;
            }

            return vm;
        }

        private async Task<ChallengeResultViewModel> PostRewardAsync(string rewardString)
        {
            ChallengeResultViewModel vm = new ChallengeResultViewModel();

            try
            {
                ClaimRewardResult response = await _rewardClient.ClaimAsync(rewardString);

                if (response != null)
                {
                    switch (response.Status)
                    {
                        case RewardStatus.Claimed:
                            vm.result = ChallengeResult.Added;
                            vm.Title = response.ViewModel.Name;
                            break;

                        case RewardStatus.Duplicate:
                            vm.result = ChallengeResult.Duplicate;
                            vm.Title = "Duplicate";
                            break;

                        case RewardStatus.Error:
                            vm.result = ChallengeResult.Error;
                            vm.Title = "Error";
                            break;

                        case RewardStatus.NotFound:
                            vm.result = ChallengeResult.NotFound;
                            vm.Title = "Unrecognised";
                            break;

                        default:
                            vm.result = ChallengeResult.Error;
                            vm.Title = "Error";
                            break;
                    }
                }
                else
                {
                    vm.result = ChallengeResult.Error;
                }
            }
            catch (ApiException e)
            {
                if (e.StatusCode == 401)
                {
                    await App.Current.MainPage.DisplayAlert("Authentication Failure", "Looks like your session has expired. Choose OK to go back to the login screen.", "OK");
                    Application.Current.MainPage = new SSW.Rewards.Views.LoginPage();
                }
                else
                {
                    vm.result = ChallengeResult.Error;
                }
            }
            catch
            {
                vm.result = ChallengeResult.Error;
            }

            return vm;
        }

        public async Task<ChallengeResultViewModel> ValidateQRCodeAsync(string qrCodeData)
        {
            var result = await PostRewardAsync(qrCodeData);

            if (result.result == ChallengeResult.Added || result.result == ChallengeResult.Duplicate)
            {
                result.ChallengeType = ChallengeType.Reward;
                return result;
            }

            result = await PostChallengeAsync(qrCodeData);
            result.ChallengeType = ChallengeType.Achievement;

            return result;
        }
    }
}