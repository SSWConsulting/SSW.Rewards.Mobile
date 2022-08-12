using FFImageLoading.Transformations;
using SSW.Rewards.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DevImageView : ContentView
    {
        public static BindableProperty ProfileProperty = BindableProperty.Create(nameof(Profile), typeof(DevProfile), typeof(DevImageView), null, propertyChanged: ProfileChanged);

        public DevImageView()
        {
            InitializeComponent();
        }

        public DevProfile Profile
        {
            get => (DevProfile)GetValue(ProfileProperty);
            set => SetValue(ProfileProperty, value);
        }

        private static void ProfileChanged(BindableObject prop, object oldVal, object newVal)
        {
            var control = (DevImageView)prop;

            control.DevImage.Source = null;
            control.DevImage.Transformations.Clear();

            var profile = (DevProfile)newVal;

            if (profile is null)
            {
                return;
            }

            if (!profile.Scanned)
            {
                control.DevImage.Transformations.Add(new GrayscaleTransformation());
            }

            control.DevImage.Source = profile.Picture;
        }
    }
}