using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SSW.Consulting.Models;
using Xamarin.Forms;

namespace SSW.Consulting.Services
{
    public class DevService : IDevService
    {
        private StaffClient _staffClient;
        private HttpClient _httpClient;
        private IUserService _userService;

        public DevService(IUserService userService)
        {
            _userService = userService;
            _httpClient = new HttpClient();
            Initialise();
        }

        private async Task Initialise()
        {
            string token = await _userService.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string baseUrl = Constants.ApiBaseUrl;

            _staffClient = new StaffClient(baseUrl, _httpClient);
        }

        public async Task<IEnumerable<DevProfile>> GetProfilesAsync()
        {
			List<DevProfile> profiles = new List<DevProfile>();

            try
            {
                StaffListViewModel profileList = await _staffClient.GetAsync();

                foreach (StaffDto profile in profileList.Staff)
                {
                    DevProfile dev = new DevProfile
                    {
                        FirstName = profile.Name,
                        Bio = profile.Profile,
                        Email = profile.Email,
                        Picture = string.IsNullOrWhiteSpace(profile.ProfilePhoto?.ToString()) ? "placeholder" : profile.ProfilePhoto.ToString(),
						Title = profile.Title,
                        TwitterID = profile.TwitterUsername,
						Skills = profile.Skills?.ToList()
					};

                    profiles.Add(dev);
                }
            }
            catch (ApiException e)
            {
                if (e.StatusCode == 401)
                {
                    await App.Current.MainPage.DisplayAlert("Authentication Failure", "Looks like your session has expired. Choose OK to go back to the login screen.", "OK");
                    Application.Current.MainPage = new SSW.Consulting.Views.LoginPage();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem loading the profiles. Please try again soon.", "OK");
                }
            }

            return profiles;
        }
    }
}
