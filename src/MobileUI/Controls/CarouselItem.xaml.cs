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