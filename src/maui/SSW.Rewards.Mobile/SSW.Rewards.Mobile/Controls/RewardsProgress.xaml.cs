namespace SSW.Rewards.Controls;

// TODO: Rebuild this control using MauiGraphics

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class RewardsProgress : ContentView
{

    public static readonly BindableProperty ProgressProperty = BindableProperty.Create(nameof(Progress), typeof(double), typeof(RewardsProgress), null, propertyChanged: ProgressChanged);
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    private float _progressAngle;

    private const int framems = 34; // number of milliseconds per step

    public RewardsProgress()
    {
        InitializeComponent();
    }

    private static async void ProgressChanged(BindableObject prop, object oldVal, object newVal)
    {

        // TODO: create an OnAppearing event and bind it to the onappearing event of the page
        // move this logic to another method
        // call the other method from both this and the event
        var progressControl = (RewardsProgress)prop;
        var targetVal = (double)newVal;

        for (int i = 15; i > 0; i--)
        {
            var thisVal = targetVal / i;
            progressControl.SetAngleFromProgress(thisVal);
            progressControl.DrawProgress();
            await Task.Delay(framems);
        }
    }

    private void DrawProgress()
    {
        ProgressBar.Drawable = new RewardsProgressBar(_progressAngle);
    }

    private void SetAngleFromProgress(double progress)
    {
        _progressAngle = (float)progress * 100f * 3.6f;
    }
}