using System.Collections.ObjectModel;
using Maui.BindableProperty.Generator.Core;

namespace SSW.Rewards.Mobile.Controls;

public partial class SegmentedControl : ContentView
{
	public SegmentedControl()
	{
		InitializeComponent();
		BindableLayout.SetItemsSource(SegmentedControlFlexLayout, _internalSegments);
	}

	public event EventHandler<Segment> SelectionChanged;

	[AutoBindable(DefaultBindingMode = "TwoWay")]
	private Segment _selectedSegment;

	[AutoBindable(OnChanged = nameof(SegmentsChanged))]
	private List<Segment> _segments;
	private void SegmentsChanged(List<Segment> segments)
	{
		_internalSegments.Clear();
		foreach (var segment in segments)
		{
            _internalSegments.Add(segment);
        }

		if (_internalSegments.Count > 0)
		{
			_internalSegments[0].IsSelected = true;
            SelectedSegment = _internalSegments[0];
			SelectionChanged?.Invoke(this, _internalSegments[0]);
        }
	}
    
    private void Segment_Tapped(object sender, TappedEventArgs e)
    {
        var segment = e.Parameter as Segment;
        
        if (segment == null)
        {
            return;
        }
        
        foreach (var item in _internalSegments)
        {
            item.IsSelected = false;
        }

        segment.IsSelected = true;
        SelectedSegment = segment;
        SelectionChanged?.Invoke(this, segment);
    }

	private readonly ObservableCollection<Segment> _internalSegments = new();
}

public class Segment
{
	public string Name { get; set; } = string.Empty;

    public object Value { get; set; }

    public bool IsSelected { get; set; }
}