using System;
using Xamarin.Forms;

namespace SSW.Rewards.Controls
{	
	public partial class Search
	{
		private const string DismissIcon = "\ue4c3";
		private const string SearchIcon = "\uea7c";
		
		public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
			nameof(BorderColor),
			typeof(Color),
			typeof(Search));
		public Color BorderColor
		{
			get => (Color)GetValue(BorderColorProperty);
			set => SetValue(BorderColorProperty, value);
		}

		public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
			nameof(TextColor),
			typeof(Color),
			typeof(Search));

		public Color TextColor
		{
			get => (Color)GetValue(TextColorProperty);
			set => SetValue(TextColorProperty, value);
		}

		public event EventHandler<string> SearchTextChanged; 

		public Search()
		{
			BindingContext = this;
			InitializeComponent();
			Icon.Text = SearchIcon;
		}
		
		private void SearchEntry_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			var searchText = SearchEntry.Text;
			Icon.Text = string.IsNullOrEmpty(searchText) ? SearchIcon : DismissIcon;
			SearchTextChanged?.Invoke(this, searchText);
		}
		
		private void Icon_OnTapped(object sender, EventArgs e)
		{
			SearchEntry.Text = string.Empty;
			Icon.Text = SearchIcon;
		}

		protected override void OnPropertyChanged(string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == nameof(BorderColor))
			{
				Border.BackgroundColor = BorderColor;
			} 
			else if (propertyName == nameof(TextColor))
			{
				SearchEntry.TextColor = TextColor;
				Icon.TextColor = TextColor;
			}
		}
	}
}

