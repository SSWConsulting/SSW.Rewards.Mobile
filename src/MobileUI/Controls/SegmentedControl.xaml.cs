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

	[AutoBindable(DefaultBindingMode = "TwoWay")]
	private Segment _selectedSegment;

    [AutoBindable(OnChanged = nameof(SegmentsChanged))]
    private List<Segment> _segments;
    private void SegmentsChanged(List<Segment> segments)
    {
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
        
        if (segment == null)
        {
            return;
        }
        
        foreach (var item in Segments)
        {
            if (item == segment)
            {
                item.IsSelected = true;
            }
            else
            {
                item.IsSelected = false;
            }
        }

        segment.IsSelected = true;
        SelectedSegment = segment;
        SelectionChanged?.Invoke(this, segment);
    }
}

public partial class Segment : ObservableObject
{
	public string Name { get; set; } = string.Empty;

    public object Value { get; set; }

    [ObservableProperty]
    private bool _isSelected;
}