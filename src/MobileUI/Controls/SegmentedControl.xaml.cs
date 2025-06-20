using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Maui.BindableProperty.Generator.Core;

namespace SSW.Rewards.Mobile.Controls;

public partial class SegmentedControl : ContentView
{
	public SegmentedControl()
	{
		InitializeComponent();
        SegmentBorder.BindingContext = this;
	}

	public event EventHandler<Segment> SelectionChanged;

	[AutoBindable(DefaultBindingMode = "TwoWay", OnChanged = nameof(SegmentChanged))]
	private Segment? _selectedSegment;

    private void SegmentChanged(Segment segment)
    {
        SetSelected(segment);
    }

    [AutoBindable(OnChanged = nameof(SegmentsChanged))]
    private List<Segment> _segments;
    private void SegmentsChanged(List<Segment> segments)
    {
        if (Segments == null)
            return;
        
        if (Segments.Count > 0)
        {
            Segments[0].IsSelected = true;
            SelectedSegment = Segments[0];
            SelectionChanged?.Invoke(this, Segments[0]);
        }

        foreach (var segment in Segments)
        {
            InternalSegments.Add(segment);
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