namespace SSW.Rewards.Mobile.Controls;

public partial class Snackbar
{
    private static SnackbarOptions defaultOptions = new()
    {
        ActionCompleted = true,
        Glyph = "\uf3ca",
        GlyphIsBrand = true,
        Message = "",
        Points = 0,
        ShowPoints = false
    };

    public Snackbar(SnackbarOptions? options = null)
    {
        InitializeComponent();
        SetOptions(options);
    }

    private void SetOptions(SnackbarOptions options)
    {
        options ??= defaultOptions;

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
        GlyphIconLabel.FontFamily = options.GlyphIsBrand ? "FA6Brands" : "FluentIcons";

        MessageLabel.Text = options.Message;
        TickLabel.IsVisible = !options.ShowPoints && options.ActionCompleted;
        PointsLabel.IsVisible = options.ShowPoints;
        PointsLabel.Text = string.Format("⭐ {0:n0}", options.Points);

        _ = SetDismissTimer();
    }

    private async Task SetDismissTimer()
    {
        await Task.Delay(5000);
        await CloseAsync();
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