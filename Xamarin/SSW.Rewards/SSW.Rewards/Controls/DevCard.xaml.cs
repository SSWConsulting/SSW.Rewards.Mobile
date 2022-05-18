using SSW.Rewards.Models;
using Xamarin.Forms;

namespace SSW.Rewards.Controls
{
    public partial class DevCard : ContentView
    {
        private DevProfile _viewModel;

        private int _selectedDevId { get; set; }

        public DevCard()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<object>(this, "SlideOut", (obj) => { SlideOut(); });
            MessagingCenter.Subscribe<object>(this, "SlideIn", (obj) => { SlideIn(); });
            MessagingCenter.Subscribe<object, int>(this, "DevChanged", (obj, args) => { DevChanged(args); });
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (this.BindingContext == null) return;
            _viewModel = this.BindingContext as DevProfile;
        }

        private void SlideIn()
        {
            int devId = int.Parse(DevName.Text);
			if (devId == _selectedDevId)
			{
				BadgeCollection.TranslateTo(0, 0);
			}
		}

        private void SlideOut()
        {
            int devId = int.Parse(DevName.Text);
			if (devId == _selectedDevId)
			{
				BadgeCollection.TranslateTo(150, 0);
			}
		}

        private void DevChanged(int id)
        {
            _selectedDevId = id;
		}
	}
}
