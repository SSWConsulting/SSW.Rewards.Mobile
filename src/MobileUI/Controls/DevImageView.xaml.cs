﻿using FFImageLoading.Transformations;

namespace SSW.Rewards.Mobile.Controls
{
    public partial class DevImageView
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

            var profile = newVal as DevProfile;
            if (profile is null)
            {
                control.DevImage.Source = "dev_placeholder.png";
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