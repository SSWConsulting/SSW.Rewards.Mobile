using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui;

namespace SSW.Rewards.Mobile.Controls;

public partial class CarouselItem
{
    [BindableProperty]
    public partial string CarouselImage { get; set; }
    
    [BindableProperty]
    public partial string Description { get; set; }
    
    [BindableProperty]
    public partial int Points { get; set; }
    
    [BindableProperty]
    public partial string ButtonText { get; set; }
    
    [BindableProperty]
    public partial IAsyncRelayCommand ButtonCommand { get; set; }
    
    [BindableProperty]
    public partial int ItemId { get; set; }
    
    [BindableProperty]
    public partial bool IsButtonDisabled { get; set; }
    
    [BindableProperty]
    public partial string RibbonText { get; set; }

    private object _cachedBindingContext;

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        // TECH DEBT: Workaround for the previous item disappearing in CarouselView on iOS
        // See: https://github.com/dotnet/maui/issues/22015
        if (BindingContext is not null)
        {
            _cachedBindingContext = BindingContext;
        }
        else
        {
            BindingContext = _cachedBindingContext;
        }
    }

    public CarouselItem()
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