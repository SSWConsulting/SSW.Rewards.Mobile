using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Snackbar : ContentView
    {
        public static readonly BindableProperty ActionComletedProperty = BindableProperty.Create(nameof(ActionCompleted), typeof(bool), typeof(Snackbar), null, propertyChanged: SetColors);

        public bool ActionCompleted
        {
            get => (bool)GetValue(ActionComletedProperty);
            set => SetValue(ActionComletedProperty, value);
        }

        public static readonly BindableProperty GlyphProperty = BindableProperty.Create(nameof(Glyph), typeof(string), typeof(Snackbar), null);
        public string Glyph
        {
            get => (string)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        public static readonly BindableProperty MessageProperty = BindableProperty.Create(nameof(Message), typeof(string), typeof(Snackbar), null);
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly BindableProperty GlyphIsBrandProperty = BindableProperty.Create(nameof(GlyphIsBrand), typeof(bool), typeof(Snackbar), null, propertyChanged: SetGlyphIcon);

        public bool GlyphIsBrand
        {
            get => (bool)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        public static readonly BindableProperty PointsProperty = BindableProperty.Create(nameof(Points), typeof(int), typeof(Snackbar), null);
        public int Points
        {
            get => (int)GetValue(PointsProperty);
            set => SetValue(PointsProperty, value);
        }

        public static readonly BindableProperty ShowPointsProperty = BindableProperty.Create(nameof(ShowPoints), typeof(bool), typeof(Snackbar), null);
        public bool ShowPoints
        {
            get => (bool)GetValue(ShowPointsProperty);
            set => SetValue(ShowPointsProperty, value);
        }

        public static readonly BindableProperty GlyphFontProperty = BindableProperty.Create(nameof(GlyphFont), typeof(string), typeof(Snackbar), null);
        public string GlyphFont
        {
            get => (string)GetValue(GlyphFontProperty);
            set => SetValue(GlyphFontProperty, value);
        }

        public static readonly BindableProperty BrandFontProperty = BindableProperty.Create(nameof(BrandFont), typeof(string), typeof(Snackbar), null);
        public string BrandFont
        {
            get => (string)GetValue(BrandFontProperty);
            set => SetValue(BrandFontProperty, value);
        }

        public bool ShowTick => !ShowPoints && ActionCompleted;

        public Snackbar()
        {
            BindingContext = this;
            InitializeComponent();
        }

        private static void SetColors(BindableObject bindable, object oldValue, object newValue)
        {
            var snack = (Snackbar)bindable;

            var completed = (bool)newValue;

            if (completed)
            {
                snack.GridBackground.BackgroundColor = Color.FromHex("cc4141");
                snack.IconLabel.BackgroundColor = Color.White;
                snack.IconLabel.TextColor = Color.FromHex("cc4141");
            }
            else
            {
                snack.GridBackground.BackgroundColor = Color.FromHex("717171");
                snack.IconLabel.TextColor = Color.White;
                snack.IconLabel.BackgroundColor = Color.FromHex("414141");
            }
        }

        private static void SetGlyphIcon(BindableObject bindable, object oldValue, object newValue)
        {
            var snack = (Snackbar)bindable;

            var isBrand = (bool)newValue;

            if (isBrand)
            {
                snack.IconLabel.FontFamily = snack.BrandFont;
            }
            else
            {
                snack.IconLabel.FontFamily = snack.GlyphFont;
            }
        }

        public void ShowSnackbar()
        {
            MainLayout.FadeTo(1, 750);
            MainLayout.TranslateTo(0, 0, 750);
        }
    }
}