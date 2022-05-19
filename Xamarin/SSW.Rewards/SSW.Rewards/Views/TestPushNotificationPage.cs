using System;

using SSW.Rewards.Services;

using Xamarin.Forms;
using Xamarin.Essentials;

namespace SSW.Rewards
{
    public partial class TestPushNotificationPage : ContentPage
    {
        readonly INotificationRegistrationService _notificationRegistrationService;

        public TestPushNotificationPage()
        {
            InitializeComponent();
            _notificationRegistrationService = ServiceContainer.Resolve<INotificationRegistrationService>();
        }

        void RegisterButtonClicked(object sender, EventArgs e) => _notificationRegistrationService
            .RegisterDeviceAsync().ContinueWith((task) => {
                ShowAlert(task.IsFaulted ? task.Exception.Message : $"Device registered");
            });

        void DeregisterButtonClicked(object sender, EventArgs e) => _notificationRegistrationService
            .DeregisterDeviceAsync().ContinueWith((task) => {
                ShowAlert(task.IsFaulted ? task.Exception.Message : $"Device deregistered");
            });

        void ShowAlert(string message) => MainThread.BeginInvokeOnMainThread(()
            => DisplayAlert("Test Push", message, "OK")
                .ContinueWith((task) => { if (task.IsFaulted) throw task.Exception; })
        );
    }
}