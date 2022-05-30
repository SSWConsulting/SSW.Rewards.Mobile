using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.Controls
{
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

        public RewardsProgress()
        {
            InitializeComponent();
        }

        private void DrawControl()
        {
            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasPaintSurface;
            Content = canvasView;
        }

        private static void ProgressChanged(BindableObject prop, object oldVal, object newVal)
        {
            var progressControl = (RewardsProgress)prop;
            progressControl.SetAngleFromProgress((double)newVal);
            progressControl.DrawControl();
        }

        private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(52,52,52), // gray background, red part rgb(204, 65, 65)
                StrokeWidth = 75
            };

            var radius = (info.Width * 0.75f) / 2; // this will make the circle take up 3/4 the width

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
}