using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SSW.Consulting.Controls
{
    public partial class FrostedGlassView : ContentView
    {
        public FrostedGlassView()
        {
            InitializeComponent();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Pink);

            SKBlurStyle blurStyle = SKBlurStyle.Normal;

            float sigma = 0.5f;

            using (SKPaint paint = new SKPaint())
            {
                paint.MaskFilter = SKMaskFilter.CreateBlur(blurStyle, sigma);

                SKRect blurRect = new SKRect(0, 0, info.Width, info.Height); //SKRect(0, 0, info.Width, textBounds.Height + 50);

                canvas.DrawRect(blurRect, paint: paint);

                //canvas.DrawBitmap(bitmap, bitmapRect, BitmapStretch.Uniform, paint: paint);
            }
        }
    }
}
