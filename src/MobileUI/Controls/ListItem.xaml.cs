using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Maui.BindableProperty.Generator.Core;

namespace SSW.Rewards.Mobile.Controls;

public partial class ListItem : Border
{
    public string ThumbnailImage
    {
        get => (string)GetValue(ThumbnailImageProperty);
        set => SetValue(ThumbnailImageProperty, value);
    }

    public static readonly BindableProperty ThumbnailImageProperty =
        BindableProperty.Create(
            nameof(ThumbnailImage),
            typeof(string),
            typeof(ListItem),
            string.Empty
        );
    
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(ListItem),
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
            typeof(ListItem),
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
            typeof(ListItem),
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
            typeof(ListItem),
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
            typeof(ListItem)
        );
    
    public bool ShowTick
    {
        get => (bool)GetValue(ShowTickProperty);
        set => SetValue(ShowTickProperty, value);
    }

    public static readonly BindableProperty ShowTickProperty =
        BindableProperty.Create(
            nameof(ShowTick),
            typeof(bool),
            typeof(ListItem),
            false
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
            typeof(ListItem),
            false
        );
    
    public ListItem()
    {
        InitializeComponent();
    }
}