using CommunityToolkit.Mvvm.Input;

namespace SSW.Rewards.Mobile.Controls;

public partial class ListItem
{
    [CommunityToolkit.Maui.BindableProperty]
    public partial string ThumbnailImage { get; set; }

    [CommunityToolkit.Maui.BindableProperty]
    public partial string PlaceholderGlyph { get; set; } = "\uf03e";

    [CommunityToolkit.Maui.BindableProperty]
    public partial string Title { get; set; }
    
    [CommunityToolkit.Maui.BindableProperty]
    public partial string Description { get; set; }

    [CommunityToolkit.Maui.BindableProperty]
    public partial int Points { get; set; }
    
    [CommunityToolkit.Maui.BindableProperty]
    public partial string ButtonText { get; set; }
    
    [CommunityToolkit.Maui.BindableProperty]
    public partial IAsyncRelayCommand ButtonCommand { get; set; }
    
    [CommunityToolkit.Maui.BindableProperty]
    public partial bool ShowTick { get; set; }
    
    [CommunityToolkit.Maui.BindableProperty]
    public partial bool IsDisabled { get; set; }
    
    [CommunityToolkit.Maui.BindableProperty]
    public partial bool IsButtonDisabled { get; set; }

    [CommunityToolkit.Maui.BindableProperty]
    public partial int ItemId { get; set; } = -1;

    public ListItem()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private async Task ButtonClicked()
    {
        if (!IsButtonDisabled && ButtonCommand != null)
        {
            await ButtonCommand.ExecuteAsync(ItemId);
        }
    }
}