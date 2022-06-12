using SSW.Rewards.Models;
using SSW.Rewards.Pages;
using SSW.Rewards.ViewModels;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SSW.Rewards.Services
{
    public class ScannerService : BaseService, IScannerService
    {
        public const string PointsAwardedMessage = "PointsAwarded";

        private AchievementClient _achievementClient { get; set; }
        private RewardClient _rewardClient { get; set; }

        public ScannerService()
        {
            _achievementClient = new AchievementClient(BaseUrl, AuthenticatedClient);
            _rewardClient = new RewardClient(BaseUrl, AuthenticatedClient);
        }

        private async Task<ScanResponseViewModel> PostAchievementAsync(string achievementString)
        {
            ScanResponseViewModel vm = new ScanResponseViewModel
            {
                ScanType = ScanType.Achievement
            };

            try
            {
                PostAchievementResult response = await _achievementClient.PostAsync(achievementString);

                if (response != null)
                {
                    switch (response.Status)
                    {
                        case AchievementStatus.Added:
                            vm.result = ScanResult.Added;
                            vm.Title = response.ViewModel.Name;
                            vm.Points = response.ViewModel.Value;
                            break;

                        case AchievementStatus.Duplicate:
                            vm.result = ScanResult.Duplicate;
                            vm.Title = "Duplicate";
                            vm.Points = 0;
                            break;

                        case AchievementStatus.Error:
                            vm.result = ScanResult.Error;
                            vm.Title = "Error";
                            vm.Points = 0;
                            break;

                        case AchievementStatus.NotFound:
                            vm.result = ScanResult.NotFound;
                            vm.Title = "Unrecognised";
                            vm.Points = 0;
                            break;
                    }
                }
                else
                    vm.result = ScanResult.Error;
            }
            catch (ApiException e)
            {
                if (e.StatusCode == 401)
                {
                    await App.Current.MainPage.DisplayAlert("Authentication Failure", "Looks like your session has expired. Choose OK to go back to the login screen.", "OK");
                    Application.Current.MainPage = new LoginPage();
                }
                else
                {
                    vm.result = ScanResult.Error;
                }
            }
            catch
            {
                vm.result = ScanResult.Error;
            }

            return vm;
        }

        private async Task<ScanResponseViewModel> PostRewardAsync(string rewardString)
        {
            ScanResponseViewModel vm = new ScanResponseViewModel
            {
                ScanType = ScanType.Reward
            };

            try
            {
                ClaimRewardResult response = await _rewardClient.ClaimAsync(rewardString);

                if (response != null)
                {
                    switch (response.Status)
                    {
                        case RewardStatus.Claimed:
                            vm.result = ScanResult.Added;
                            vm.Title = response.ViewModel.Name;
                            break;

                        case RewardStatus.Duplicate:
                            vm.result = ScanResult.Duplicate;
                            vm.Title = "Duplicate";
                            break;

                        case RewardStatus.Error:
                            vm.result = ScanResult.Error;
                            vm.Title = "Error";
                            break;

                        case RewardStatus.NotFound:
                            vm.result = ScanResult.NotFound;
                            vm.Title = "Unrecognised";
                            break;

                        default:
                            vm.result = ScanResult.Error;
                            vm.Title = "Error";
                            break;
                    }
                }
                else
                {
                    vm.result = ScanResult.Error;
                }
            }
            catch (ApiException e)
            {
                if (e.StatusCode == 401)
                {
                    await App.Current.MainPage.DisplayAlert("Authentication Failure", "Looks like your session has expired. Choose OK to go back to the login screen.", "OK");
                    Application.Current.MainPage = new LoginPage();
                }
                else
                {
                    vm.result = ScanResult.Error;
                }
            }
            catch
            {
                vm.result = ScanResult.Error;
            }

            return vm;
        }

        public async Task<ScanResponseViewModel> ValidateQRCodeAsync(string qrCodeData)
        {
            if (qrCodeData.StartsWith("ach:"))
            {
                return await PostAchievementAsync(qrCodeData);
            }
            else if (qrCodeData.StartsWith("rwd:"))
            {
                return await PostRewardAsync(qrCodeData);
            }
            else
            {
                return new ScanResponseViewModel
                {
                    result = ScanResult.NotFound,
                    Title = "Unrecognised"
                };
            }
        }
    }
}