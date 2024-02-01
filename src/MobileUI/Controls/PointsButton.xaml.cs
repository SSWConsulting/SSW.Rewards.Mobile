using CommunityToolkit.Mvvm.ComponentModel;
using Maui.BindableProperty.Generator.Core;

namespace SSW.Rewards.Mobile.Controls;

public partial class PointsButton : ContentView
{
    public int Points
    {
        get => (int)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public static readonly BindableProperty PointsProperty =
        BindableProperty.Create(
            nameof(Points),
            typeof(int),
            typeof(PointsButton),
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
            typeof(PointsButton),
            string.Empty
        );
    
    public bool IsDisabled
    {
        get => (bool)GetValue(IsDisabledProperty);
        set => SetValue(IsDisabledProperty, value);
    }

    public static readonly BindableProperty IsDisabledProperty =
        BindableProperty.Create(
            nameof(IsDisabled),
            typeof(bool),
            typeof(PointsButton),
            false
        );
    
    public PointsButton()
    {
        InitializeComponent();
    }
}