using CoreGraphics;
using Microsoft.Maui.Controls.Platform.Compatibility;
using SSW.Rewards.Mobile.Controls;
using UIKit;
using Microsoft.Maui.Platform;

namespace SSW.Rewards.Mobile.Renderers;

internal class CustomShellItemRenderer(IShellContext context) : ShellItemRenderer(context)
{
    private const float ButtonSize = 70f;
    private const float ButtonPadding = 30f;
    private const float BorderWidth = 4f;
    private UIButton? _middleView;

    public override async void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();
        if (View is null || ShellItem is not CustomTabBar { CenterViewVisible: true } tabBar)
        {
            return;
        }

        _middleView?.RemoveFromSuperview();

        if (_middleView is null)
        {
            var context = tabBar.Window?.Page?.Handler?.MauiContext ?? Application.Current?.Windows.LastOrDefault()?.Page?.Handler?.MauiContext;
            var image = await tabBar.CenterViewImageSource.GetPlatformImageAsync(context!);

            _middleView = new UIButton(UIButtonType.Custom);
            _middleView.BackgroundColor = tabBar.CenterViewBackgroundColor?.ToPlatform();
            _middleView.Frame = new CGRect(CGPoint.Empty, new CGSize(ButtonSize, ButtonSize));
            if (image is not null)
            {
                _middleView.SetImage(image.Value, UIControlState.Normal);
                _middleView.Frame = new CGRect(CGPoint.Empty, image.Value.Size + new CGSize(ButtonPadding, ButtonPadding));
            }

            _middleView.AutoresizingMask = UIViewAutoresizing.FlexibleRightMargin |
                                           UIViewAutoresizing.FlexibleLeftMargin |
                                           UIViewAutoresizing.FlexibleBottomMargin;
            _middleView.Layer.CornerRadius = _middleView.Frame.Width / 2;
            _middleView.Layer.BorderWidth = BorderWidth;
            _middleView.Layer.BorderColor = UIColor.White.CGColor;
            _middleView.Layer.MasksToBounds = false;

            _middleView.TouchUpInside += delegate
            {
                tabBar.CenterView_Tapped();
            };;
        }

        _middleView.Center = new CGPoint(View.Bounds.GetMidX(), TabBar.Frame.Top + BorderWidth);

        View.AddSubview(_middleView);
    }
}