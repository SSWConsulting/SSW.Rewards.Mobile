using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;
using System.Collections.ObjectModel;

namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public class MyProfileViewModel : ProfileViewModelBase, 
    IRecipient<ProfilePicUpdatedMessage>, 
    IRecipient<PointsAwardedMessage>,
    IRecipient<SocialUsernameAddedMessage>,
    IRecipient<UserDetailsUpdatedMessage>
{
    public MyProfileViewModel(IRewardService rewardsService, IUserService userService, ISnackbarService snackbarService) : base(rewardsService, userService, snackbarService)
    {
    }

    public async void Receive(PointsAwardedMessage message)
    {
        await OnPointsAwarded();
    }

    public void Receive(SocialUsernameAddedMessage message)
    {
        AddSocialMediaId(message);
    }
    
    public async void Receive(UserDetailsUpdatedMessage message)
    {
        await UpdateUserData();
    }

    public async Task Initialise()
    {
        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProfileSections = new ObservableCollection<ProfileCarouselViewModel>();
                OnPropertyChanged(nameof(ProfileSections));
            });
        }

        _isMe = true;

        await _initialise();
    }
    
    public new async Task OnAppearing()
    {
        await UpdateUserData();
    }

    private void AddSocialMediaId(SocialUsernameAddedMessage message)
    {
        // We will remove this. In v3 we're doing social media integration at the identity level.
        return;

        //WeakReferenceMessenger.Default.Unregister<SocialUsernameAddedMessage>(this);

        //IsBusy = true;

        //var result = await _userService.SaveSocialMediaId(message.Value.Achievement.Id, message.Value.Username);

        //var options = new SnackbarOptions
        //{
        //    ActionCompleted = false,
        //    Points = message.Value.Achievement.Value,
        //    GlyphIsBrand = message.Value.Achievement.IconIsBranded,
        //    Glyph = (string)typeof(Icon).GetField(message.Value.Achievement.AchievementIcon.ToString()).GetValue(null)
        //};

        //if (result)
        //{
        //    options.ActionCompleted = true;
        //    message.Value.Achievement.Complete = true;
        //}

        //options.Message = $"{GetMessage(message.Value.Achievement)}";

        //await _snackbarService.ShowSnackbar(options);

        //if (result)
        //{
        //    WeakReferenceMessenger.Default.Send(new PointsAwardedMessage());
        //}

        //IsBusy = false;
    }

    private async Task UpdateUserData()
    {
        ProfilePic = _userService.MyProfilePic;
        Name = _userService.MyName;
        Points = _userService.MyPoints;
        Balance = _userService.MyBalance;
        userId = _userService.MyUserId;

        await UpdatePoints();
        await LoadProfileSections();
    }

    private async Task OnPointsAwarded()
    {
        await _userService.UpdateMyDetailsAsync();
        await UpdatePoints();
        await LoadProfileSections();
    }

    private Task UpdatePoints()
    {
        Points = _userService.MyPoints;
        Balance = _userService.MyBalance;

        double progress = Balance / _topRewardCost;

        // TODO: we can get rid of this 0 condition if we award a 'sign up'
        // achievement. We could also potentially get the ring to render
        // empty.
        if (progress == 0)
        {
            Progress = 0.01;
        }
        else if (progress < 1)
        {
            Progress = progress;
        }
        else
        {
            Progress = 1;
        }

        return Task.CompletedTask;
    }
}
