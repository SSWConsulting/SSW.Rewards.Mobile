namespace SSW.Rewards.Mobile.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Snackbar : ContentView
    {
        public static readonly BindableProperty OptionsProperty = BindableProperty.Create(nameof(Options), typeof(SnackbarOptions), typeof(Snackbar), DefaultOptions, propertyChanged: OptionsChanged);
        public SnackbarOptions Options
        {
            get => (SnackbarOptions)GetValue(OptionsProperty);
            set => SetValue(OptionsProperty, value);
        }

        private static SnackbarOptions DefaultOptions = new SnackbarOptions
        {
            ActionCompleted = true,
            Glyph = "\uf3ca",
            GlyphIsBrand = true,
            Message = "",
            Points = 0,
            ShowPoints = false
        };

        public Snackbar()
        {
            InitializeComponent();
        }

        private static void OptionsChanged(BindableObject bindable, object oldVal, object newVal)
        {
            var snack = (Snackbar)bindable;

            var opt = (SnackbarOptions)newVal;

            if (opt.ActionCompleted)
            {
                snack.GridBackground.BackgroundColor = Color.FromArgb("cc4141");
                snack.BrandIconLabel.BackgroundColor = Colors.White;
                snack.BrandIconLabel.TextColor = Color.FromArgb("cc4141");
                snack.GlyphIconLabel.BackgroundColor = Colors.White;
                snack.GlyphIconLabel.TextColor = Color.FromArgb("cc4141");
            }
            else
            {
                snack.GridBackground.BackgroundColor = Color.FromArgb("717171");
                snack.BrandIconLabel.TextColor = Colors.White;
                snack.BrandIconLabel.BackgroundColor = Color.FromArgb("414141");
                snack.GlyphIconLabel.TextColor = Colors.White;
                snack.GlyphIconLabel.BackgroundColor = Color.FromArgb("414141");
            }

            if (opt.GlyphIsBrand)
            {
                snack.BrandIconLabel.Text = opt.Glyph;
                snack.BrandIconLabel.IsVisible = true;
                snack.GlyphIconLabel.IsVisible = false;
            }
            else
            {
                snack.GlyphIconLabel.Text = opt.Glyph;
                snack.BrandIconLabel.IsVisible = false;
                snack.GlyphIconLabel.IsVisible = true;
            }

            snack.MessageLabel.Text = opt.Message;
            snack.TickLabel.IsVisible = !opt.ShowPoints && opt.ActionCompleted;
            snack.PointsLabel.IsVisible = opt.ShowPoints;
            snack.PointsLabel.Text = string.Format("⭐ {0:n0}", opt.Points);
        }

        private bool _isShowing = false;

        public async Task ShowSnackbar()
        {
            if (!_isShowing)
            {
                _isShowing = true;
                MainLayout.InputTransparent = false;
                MainLayout.FadeTo(1, 750);
                MainLayout.TranslateTo(0, 0, 750);

                await Task.Delay(5000);
                MainLayout.FadeTo(0, 750);
                MainLayout.TranslateTo(0, 50, 750);
                MainLayout.InputTransparent = true;
                _isShowing = false;
            }
        }
    }

    public class SnackbarOptions
    {
        public bool ActionCompleted { get; set; }

        public string Glyph { get; set; }

        public string Message { get; set; }

        public bool GlyphIsBrand { get; set; }

        public int Points { get; set; }

        public bool ShowPoints { get; set; }
    }

    public class ShowSnackbarEventArgs : EventArgs
    {
        public SnackbarOptions Options { get; set; }
    }
}