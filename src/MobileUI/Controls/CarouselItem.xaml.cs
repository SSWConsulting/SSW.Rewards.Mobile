using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Maui.BindableProperty.Generator.Core;

namespace SSW.Rewards.Mobile.Controls;

public partial class CarouselItem : Border
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
    
    public ICommand ButtonCommand
    {
        get => (ICommand)GetValue(ButtonCommandProperty);
        set => SetValue(ButtonCommandProperty, value);
    }

    public static readonly BindableProperty ButtonCommandProperty =
        BindableProperty.Create(
            nameof(ButtonCommand),
            typeof(ICommand),
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
    
    public CarouselItem()
    {
        InitializeComponent();
    }
}