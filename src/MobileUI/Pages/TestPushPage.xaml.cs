﻿namespace SSW.Rewards.Mobile.Pages
{
    public partial class TestPushPage : ContentPage
    {
        readonly INotificationRegistrationService _notificationRegistrationService;

        public TestPushPage(INotificationRegistrationService notificationRegistrationService)
        {
            InitializeComponent();
            _notificationRegistrationService = notificationRegistrationService;
        }

        //TODO: registrationGroupTag should be based on current user role (i.e. Admin, Staff, User, etc.)
        //registrationGroupTag determines which push notifications the current user can receive.
        string registrationGroupTag = "user";
        void RegisterButtonClicked(object sender, EventArgs e) => _notificationRegistrationService
            .RegisterDeviceAsync(registrationGroupTag).ContinueWith((task) => {
                ShowAlert(task.IsFaulted ? task.Exception.Message : $"Device registered");
            });

        void DeregisterButtonClicked(object sender, EventArgs e) => _notificationRegistrationService
            .DeregisterDeviceAsync().ContinueWith((task) => {
                ShowAlert(task.IsFaulted ? task.Exception.Message : $"Device deregistered");
            });

        void ShowAlert(string message) => MainThread.BeginInvokeOnMainThread(()
            => DisplayAlert("Device Status", message, "OK")
                .ContinueWith((task) => { if (task.IsFaulted) throw task.Exception; })
        );
    }
}
