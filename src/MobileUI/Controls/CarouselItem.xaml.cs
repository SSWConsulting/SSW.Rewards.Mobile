using CommunityToolkit.Mvvm.Input;
using Maui.BindableProperty.Generator.Core;

namespace SSW.Rewards.Mobile.Controls;

public partial class CarouselItem
{
    [AutoBindable]
    private readonly string _carouselImage;
    
    [AutoBindable]
    private readonly string _description;
    
    [AutoBindable]
    private readonly int _points;
    
    [AutoBindable]
    private readonly string _buttonText;
    
    [AutoBindable]
    private readonly IAsyncRelayCommand _buttonCommand;
    
    [AutoBindable]
    private readonly int _itemId;
    
    [AutoBindable]
    private readonly bool _isButtonDisabled;
    
    [AutoBindable]
    private readonly string _ribbonText;

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