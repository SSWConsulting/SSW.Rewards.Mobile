using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Mobile.Controls;

namespace SSW.Rewards.Mobile.ViewModels;

public enum NetworkingPageSegments
{
    Friends,
    ToMeet,
    SSW,
    Other
}

public partial class NetworkingPageViewModel : BaseViewModel
{
    public NetworkingPageSegments CurrentSegment { get; set; }

    [ObservableProperty]
    private List<Segment> _segments;
    
    [ObservableProperty]
    private Segment _selectedSegment;
    
    public async Task Initialise()
    {
        if (Segments is null || Segments.Count() == 0)
        {
            Segments = new List<Segment>
            {
                new() { Name = "Friends", Value = NetworkingPageSegments.Friends },
                new() { Name = "To Meet", Value = NetworkingPageSegments.ToMeet },
                new() { Name = "SSW", Value = NetworkingPageSegments.SSW }
                // new() { Name = "This Year", Value = NetworkingPageSegments.Other },
            };
        }
    }

    [RelayCommand]
    private async Task FilterBySegment()
    {
        CurrentSegment = (NetworkingPageSegments)SelectedSegment.Value;
    }
}