using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui;

namespace SSW.Rewards.Mobile.Controls;

public partial class ListItem : Border
{
    [BindableProperty]
    public partial string ThumbnailImage { get; set; }

    [BindableProperty]
    public partial string PlaceholderGlyph { get; set; } = "\uf03e";

    [BindableProperty]
    public partial string Title { get; set; }
    
    [BindableProperty]
    public partial string Description { get; set; }

    [BindableProperty]
    public partial int Points { get; set; }
    
    [BindableProperty]
    public partial string ButtonText { get; set; }
    
    [BindableProperty]
    public partial IAsyncRelayCommand ButtonCommand { get; set; }
    
    [BindableProperty]
    public partial bool ShowTick { get; set; }
    
    [BindableProperty]
    public partial bool IsDisabled { get; set; }
    
    [BindableProperty]
    public partial bool IsButtonDisabled { get; set; }

    [BindableProperty]
    public partial int ItemId { get; set; } = -1;

    public ListItem()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private async Task ButtonClicked()
    {
        if (ButtonCommand != null)
        {
            await ButtonCommand.ExecuteAsync(ItemId);
        }
    }
}
