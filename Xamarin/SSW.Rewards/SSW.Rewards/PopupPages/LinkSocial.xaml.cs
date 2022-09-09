using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SSW.Rewards.Models;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinkSocial : PopupPage
    {
        private readonly Achievement _achievement;

        public LinkSocial(Achievement achievement)
        {
            InitializeComponent();
            SaveButton.IsEnabled = false;
            _achievement = achievement;
            TitleLabel.Text = $"Enter your {achievement.Name.Split(' ').Last()} username";
        }

        private async void SaveButton_Clicked(object sender, System.EventArgs e)
        {
            var msg = new SocialUsernameMessage
            {
                Achievement = _achievement,
                Username    = UsernameEntry.Text
            };

            MessagingCenter.Send<object, SocialUsernameMessage>(this, SocialUsernameMessage.SocialUsernameAddedMessage, msg);

            await PopupNavigation.Instance.PopAllAsync();
        }

        private void UsernameEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                SaveButton.IsEnabled = false;
            }
            else
            {
                SaveButton.IsEnabled = true;
            }
        }
    }
}