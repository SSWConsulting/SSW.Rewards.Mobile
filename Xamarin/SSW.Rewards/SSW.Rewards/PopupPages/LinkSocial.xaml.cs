using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SSW.Rewards.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinkSocial : PopupPage
    {
        private readonly string _platform;

        public LinkSocial(string platform)
        {
            InitializeComponent();
            TitleLabel.Text = $"Add your {platform} username";
            SaveButton.IsEnabled = false;
            _platform = platform;
        }

        private async void SaveButton_Clicked(object sender, System.EventArgs e)
        {
            var msg = new SocialUsernameMessage
            {
                PlatformName    = _platform,
                Username        = UsernameEntry.Text
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