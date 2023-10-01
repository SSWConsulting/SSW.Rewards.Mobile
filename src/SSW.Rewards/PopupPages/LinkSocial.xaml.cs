using CommunityToolkit.Mvvm.Messaging;
using Mopups.Pages;
using Mopups.Services;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Mobile.Models;

namespace SSW.Rewards.PopupPages
{
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
            var msg = new SocialUsernameAddedMessage(new SocialAchievement
            {
                Achievement = _achievement,
                Username = UsernameEntry.Text
            });

            WeakReferenceMessenger.Default.Send(msg);

            await MopupService.Instance.PopAllAsync();
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