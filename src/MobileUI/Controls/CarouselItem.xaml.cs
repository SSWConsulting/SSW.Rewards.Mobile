using CommunityToolkit.Mvvm.Input;

namespace SSW.Rewards.Mobile.Controls;

public partial class CarouselItem
{
    public string CarouselImage
    {
        get => (string)GetValue(CarouselImageProperty);
        set => SetValue(CarouselImageProperty, value);
    }

    public static readonly BindableProperty CarouselImageProperty =
        BindableProperty.Create(
            nameof(CarouselImage),
            typeof(string),
            typeof(CarouselItem),
            string.Empty
        );
    
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(
            nameof(Description),
            typeof(string),
            typeof(CarouselItem),
            string.Empty
        );
    
    public int Points
    {
        get => (int)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public static readonly BindableProperty PointsProperty =
        BindableProperty.Create(
            nameof(Points),
            typeof(int),
            typeof(CarouselItem),
            0
        );
    
    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public static readonly BindableProperty ButtonTextProperty =
        BindableProperty.Create(
            nameof(ButtonText),
            typeof(string),
            typeof(CarouselItem),
            string.Empty
        );
    
    public IAsyncRelayCommand ButtonCommand
    {
        get => (IAsyncRelayCommand)GetValue(ButtonCommandProperty);
        set => SetValue(ButtonCommandProperty, value);
    }

    public static readonly BindableProperty ButtonCommandProperty =
        BindableProperty.Create(
            nameof(ButtonCommand),
            typeof(IAsyncRelayCommand),
            typeof(CarouselItem)
        );
    
    public int ItemId
    {
        get => (int)GetValue(ItemIdProperty);
        set => SetValue(ItemIdProperty, value);
    }

    public static readonly BindableProperty ItemIdProperty =
        BindableProperty.Create(
            nameof(ItemId),
            typeof(int),
            typeof(CarouselItem),
            -1
        );
    
    public bool IsButtonDisabled
    {
        get => (bool)GetValue(IsButtonDisabledProperty);
        set => SetValue(IsButtonDisabledProperty, value);
    }

    public static readonly BindableProperty IsButtonDisabledProperty =
        BindableProperty.Create(
            nameof(IsButtonDisabled),
            typeof(bool),
            typeof(CarouselItem),
            false
        );
    
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