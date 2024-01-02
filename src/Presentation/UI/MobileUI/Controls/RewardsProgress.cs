using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace SSW.Rewards.Mobile.Controls;

public class RewardsProgress : ContentView
{
    public static readonly BindableProperty ProgressProperty = BindableProperty.Create(nameof(Progress), typeof(double), typeof(RewardsProgress), null, propertyChanged: ProgressChanged);
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    private float _progressAngle;

    private void DrawControl()
    {
        SKCanvasView canvasView = new SKCanvasView();
        canvasView.PaintSurface += OnCanvasPaintSurface;
        Content = canvasView;
    }

    private static async void ProgressChanged(BindableObject prop, object oldVal, object newVal)
    {

        // TODO: create an OnAppearing event and bind it to the onappearing event of the page
        // move this logic to another method
        // call the other method from both this and the event
        var progressControl = (RewardsProgress)prop;

        var framems = 34; // number of milliseconds per step
        var targetVal = (double)newVal;

        for (int i = 15; i > 0; i--)
        {
            var thisVal = targetVal / i;
            progressControl.SetAngleFromProgress(thisVal);
            progressControl.DrawControl();
            await System.Threading.Tasks.Task.Delay(framems);
        }
    }

    private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs args)
    {
        SKImageInfo info = args.Info;
        SKSurface surface = args.Surface;
        SKCanvas canvas = surface.Canvas;

        canvas.Clear();

        var strokeWidth = DeviceInfo.Platform == DevicePlatform.iOS ? 30 : 60;
        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(52, 52, 52), // gray background, red part rgb(204, 65, 65)
            StrokeWidth = strokeWidth
        };

        
        var shortestSide = Math.Min(info.Height, info.Width); // if W & H are different, we need to take the shortest one to prevent overflow
        var radius = (shortestSide / 2) - strokeWidth / 2; // as the stroke is wide, the reference point is in the middle

        canvas.DrawCircle(info.Width / 2, info.Height / 2, radius, paint);

        var rectTop = (info.Height / 2) - radius;
        var rectBottom = (info.Height / 2) + radius;
        var rectRight = (info.Width / 2) + radius;
        var rectLeft = (info.Width / 2) - radius;

        SKRect rect = new SKRect(rectLeft, rectTop, rectRight, rectBottom);

        paint.Color = new SKColor(204, 65, 65);

        using (SKPath path = new SKPath())
        {
            path.AddArc(rect, -90, _progressAngle);
            canvas.DrawPath(path, paint);
        }
    }

    private void SetAngleFromProgress(double progress)
    {
        _progressAngle = (float)progress * 100f * 3.6f;
    }
}