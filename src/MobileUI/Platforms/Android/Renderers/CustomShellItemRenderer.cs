using SSW.Rewards.Mobile.Controls;
using Color = Android.Graphics.Color;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using Button = Android.Widget.Button;
using View = Android.Views.View;

namespace SSW.Rewards.Mobile.Renderers;

internal class CustomShellItemRenderer(IShellContext context) : ShellItemRenderer(context)
{
    private const int MiddleViewSizeDp = 80;
    private const int ButtonSizeDp = 50;
    private const int StrokeWidthDp = 4;
    private const int BottomMarginAdjustmentDp = 15;
    private const int BaseBottomMarginDp = 8;

    public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
    {
        var view = base.OnCreateView(inflater, container, savedInstanceState);
        
        if (Context is null || ShellItem is not CustomTabBar { CenterViewVisible: true } tabBar)
            return view;

        var density = Context.Resources?.DisplayMetrics?.Density ?? 1.0f;
        var rootLayout = CreateRootLayout();
        rootLayout.AddView(view);

        var middleViewLayoutParams = CreateMiddleViewLayoutParams(density);
        var middleView = CreateMiddleView(tabBar, middleViewLayoutParams);
        
        if (tabBar.CenterViewBackgroundColor is not null)
        {
            AddBackgroundView(rootLayout, tabBar.CenterViewBackgroundColor, middleViewLayoutParams, density);
        }

        LoadCenterViewImage(tabBar, middleView, density, middleViewLayoutParams);
        rootLayout.AddView(middleView);
        
        return rootLayout;
    }

    private FrameLayout CreateRootLayout() =>
        new(Context)
        {
            LayoutParameters = new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent, 
                ViewGroup.LayoutParams.MatchParent)
        };

    private static FrameLayout.LayoutParams CreateMiddleViewLayoutParams(float density) =>
        new(
            ViewGroup.LayoutParams.WrapContent,
            ViewGroup.LayoutParams.WrapContent,
            GravityFlags.CenterHorizontal | GravityFlags.Bottom)
        {
            BottomMargin = (int)(BaseBottomMarginDp * density),
            Width = (int)(MiddleViewSizeDp * density),
            Height = (int)(MiddleViewSizeDp * density)
        };

    private Button CreateMiddleView(CustomTabBar tabBar, FrameLayout.LayoutParams layoutParams)
    {
        var button = new Button(Context) { LayoutParameters = layoutParams };
        button.Click += delegate { tabBar.CenterView_Tapped(); };
        button.SetPadding(0, 0, 0, 0);
        return button;
    }

    private void AddBackgroundView(FrameLayout rootLayout, Microsoft.Maui.Graphics.Color backgroundColor,
        FrameLayout.LayoutParams layoutParams, float density)
    {
        var backgroundView = new View(Context) { LayoutParameters = layoutParams };
        var backgroundDrawable = new GradientDrawable();
        backgroundDrawable.SetShape(ShapeType.Rectangle);
        
        var strokeWidthPx = (int)(StrokeWidthDp * density);
        backgroundDrawable.SetStroke(strokeWidthPx, Color.White);
        backgroundDrawable.SetCornerRadius((MiddleViewSizeDp * density) / 2f);
        backgroundDrawable.SetColor(backgroundColor.ToPlatform(Colors.Transparent));
        
        backgroundView.SetBackground(backgroundDrawable);
        rootLayout.AddView(backgroundView);
    }

    private static void LoadCenterViewImage(CustomTabBar tabBar, Button middleView, float density, 
        FrameLayout.LayoutParams originalLayoutParams)
    {
        var context = tabBar.Window?.Page?.Handler?.MauiContext ?? 
                     Application.Current?.Windows.LastOrDefault()?.Page?.Handler?.MauiContext;
                     
        tabBar.CenterViewImageSource?.LoadImage(context!, result =>
        {
            if (result?.Value is not BitmapDrawable drawable || drawable.Bitmap is null)
                return;

            var buttonSizePx = (int)(ButtonSizeDp * density);
            middleView.LayoutParameters = new FrameLayout.LayoutParams(
                buttonSizePx, buttonSizePx,
                GravityFlags.CenterHorizontal | GravityFlags.Bottom)
            {
                BottomMargin = originalLayoutParams.BottomMargin + (int)(BottomMarginAdjustmentDp * density)
            };
            
            middleView.SetBackground(drawable);
            middleView.SetMinimumHeight(0);
            middleView.SetMinimumWidth(0);
        });
    }
}