using CommunityToolkit.Maui.Views;

namespace SSW.Rewards.Mobile.Controls
{
    public partial class Snackbar : Popup
    {
        private static SnackbarOptions DefaultOptions = new SnackbarOptions
        {
            ActionCompleted = true,
            Glyph = "\uf3ca",
            GlyphIsBrand = true,
            Message = "",
            Points = 0,
            ShowPoints = false
        };

        private bool _isDismissed = false;

        public Snackbar(SnackbarOptions? options = null)
        {
            InitializeComponent();

            SetOptions(options);
        }

        private void SetOptions(SnackbarOptions options)
        {
            if (options == null)
            {
                options = DefaultOptions;
            }

            if (options.ActionCompleted)
            {
                GridBackground.BackgroundColor = Color.FromArgb("cc4141");
                GlyphIconLabel.BackgroundColor = Colors.White;
                GlyphIconLabel.TextColor = Color.FromArgb("cc4141");
            }
            else
            {
                GridBackground.BackgroundColor = Color.FromArgb("717171");
                GlyphIconLabel.TextColor = Colors.White;
                GlyphIconLabel.BackgroundColor = Color.FromArgb("414141");
            }


            GlyphIconLabel.Text = options.Glyph;

            if (options.GlyphIsBrand)
            {
                GlyphIconLabel.FontFamily = "FA6Brands";
            }
            else
            {
                GlyphIconLabel.FontFamily = "FluentIcons";
            }

            MessageLabel.Text = options.Message;
            TickLabel.IsVisible = !options.ShowPoints && options.ActionCompleted;
            PointsLabel.IsVisible = options.ShowPoints;
            PointsLabel.Text = string.Format("⭐ {0:n0}", options.Points);

            _ = SetDismissTimer();
        }

        private async Task SetDismissTimer()
        {
            await Task.Delay(5000);

            //if (!_isDismissed)
            //{
                this.Close();
            //}
        }

        protected override Task OnDismissedByTappingOutsideOfPopup()
        {
            _isDismissed = true;

            // TECH DEBT: Due to this issue: https://github.com/CommunityToolkit/Maui/issues/1443
            //            we need to dismiss the modal navigation stack on iOS, as it prevents
            //            some knock on effects. See: https://github.com/SSWConsulting/SSW.Rewards.Mobile/issues/465
            //            Once this is resolved, we can get rid of these compiler directives and just use Close()
#if IOS
            return Task.FromResult(() =>
            {
                this.Close();
            });
#else
            return base.OnDismissedByTappingOutsideOfPopup();
#endif
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