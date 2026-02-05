using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Maui;

namespace SSW.Rewards.Mobile.Controls;

public partial class SegmentedControl : ContentView
{
	public SegmentedControl()
	{
		InitializeComponent();
        SegmentBorder.BindingContext = this;
	}

	public event EventHandler<Segment> SelectionChanged;

    [BindableProperty(DefaultBindingMode = BindingMode.TwoWay, PropertyChangedMethodName = nameof(SegmentChanged))]
    public partial Segment SelectedSegment { get; set; }

    private static void SegmentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (SegmentedControl)bindable;
        var segment = (Segment)newValue;
    
        control.SetSelected(segment);
    }

    
    [BindableProperty(PropertyChangedMethodName = nameof(SegmentsChanged))]
    public partial List<Segment> Segments { get; set; }

    private static void SegmentsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (SegmentedControl)bindable;
        var segments = (List<Segment>)newValue;
    
        if (segments == null)
            return;

        control.InternalSegments.Clear();

        if (segments.Count > 0)
        {
            segments[0].IsSelected = true;
            control.SelectedSegment = segments[0];
            control.SelectionChanged?.Invoke(control, segments[0]);
        }

        foreach (var segment in segments)
        {
            control.InternalSegments.Add(segment);
        }
    }
    
    public ObservableCollection<Segment> InternalSegments { get; set; } = new ();
    
    private void Segment_Tapped(object sender, TappedEventArgs e)
    {
        var segment = e.Parameter as Segment;
        
        SetSelected(segment);
        
        SelectedSegment = segment;
        SelectionChanged?.Invoke(this, segment);
    }

    private void SetSelected(Segment segment)
    {
        if (segment == null)
        {
            return;
        }
        
        foreach (var item in Segments)
        {
            item.IsSelected = item == segment;
        }

        segment.IsSelected = true;
    }
}

public partial class Segment : ObservableObject
{
	public string Name { get; set; } = string.Empty;

    public ImageSource Icon { get; set; } = null;

    public object Value { get; set; }

    [ObservableProperty]
    private bool _isSelected;
}