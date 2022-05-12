using SSW.Rewards.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SSW.Rewards.Services
{
    public class DevService : BaseService, IDevService
    {
        private StaffClient _staffClient;

        public DevService()
        {
            _staffClient = new StaffClient(BaseUrl, AuthenticatedClient);
        }

        public async Task<IEnumerable<DevProfile>> GetProfilesAsync()
        {
			List<DevProfile> profiles = new List<DevProfile>();
            int id = 0;

            try
            {
                StaffListViewModel profileList = await _staffClient.GetAsync();

                foreach (StaffDto profile in profileList.Staff.Where(s => !s.IsDeleted))
                {
                    DevProfile dev = new DevProfile
                    {
                        id = id,
                        FirstName = profile.Name,
                        Bio = profile.Profile,
                        Email = profile.Email,
                        Picture = string.IsNullOrWhiteSpace(profile.ProfilePhoto?.ToString()) ? "dev_placeholder" : profile.ProfilePhoto.ToString(),
						Title = profile.Title,
                        TwitterID = profile.TwitterUsername,
						Skills = profile.Skills?.ToList(),
                        IsExternal = profile.IsExternal
					};

                    profiles.Add(dev);
                    id++;
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
                    await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem loading the profiles. Please try again soon.", "OK");
                }
            }

            return profiles.OrderBy(d => d.FirstName);
        }
    }
}
