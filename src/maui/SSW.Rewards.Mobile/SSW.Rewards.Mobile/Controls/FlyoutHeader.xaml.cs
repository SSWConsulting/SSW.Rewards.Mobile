using CommunityToolkit.Mvvm.Messaging;
using Maui.BindableProperty.Generator.Core;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Mobile.ViewModels;

namespace SSW.Rewards.Mobile.Controls;

public partial class FlyoutHeader : ContentView, /*IRecipient<UserDetailsUpdatedMessage>,*/ IRecipient<ProfilePicUpdatedMessage>
{
    [AutoBindable(OnChanged = nameof(UpdateStringProperty))]
    private string profilePic;

    [AutoBindable(OnChanged = nameof(UpdateStringProperty))]
    private string name;

    [AutoBindable(OnChanged = nameof(UpdateStringProperty))]
    private string email;

    [AutoBindable]
    private bool staff;

    public FlyoutHeader(FlyoutHeaderViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        Console.WriteLine($"[FlyoutHeader]Initial binding value: {ProfilePic}");
        Console.WriteLine($"[FlyoutHeader]Initial binding value: {Name}");
        Console.WriteLine($"[FlyoutHeader]Initial binding value: {Email}");

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    //public void Receive(UserDetailsUpdatedMessage message)
    //{
    //    ProfilePic = message.ProfilePic;
    //    Name = message.Name;
    //    Email = message.Email;
    //    Staff = message.IsStaff;
    //}

    public void Receive(ProfilePicUpdatedMessage message)
    {
        ProfilePic = message.ProfilePic;
    }

    private void UpdateStringProperty(string newValue)
    {
        Console.WriteLine($"[FlyoutHeader] Received new binding value: {newValue}");
    }
}
