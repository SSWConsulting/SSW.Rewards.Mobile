using CoreGraphics;
using Microsoft.Maui.Controls.Platform.Compatibility;
using SSW.Rewards.Mobile.Controls;
using UIKit;

namespace SSW.Rewards.Mobile.Renderers;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

internal class CustomShellItemRenderer(IShellContext context) : ShellItemRenderer(context)
{
    private UIButton? middleView;

    public override async void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();
        if (View is not null && ShellItem is CustomTabBar { CenterViewVisible: true } tabbar)
        {
            if (middleView is not null)
            {
                middleView.RemoveFromSuperview();
            }

            if (middleView is null)
            {
                var context = tabbar.Window?.Page?.Handler?.MauiContext ?? Application.Current?.Windows.LastOrDefault()?.Page?.Handler?.MauiContext;
                var image = await tabbar.CenterViewImageSource.GetPlatformImageAsync(context!);

                middleView = new UIButton(UIButtonType.Custom);
                middleView.BackgroundColor = tabbar.CenterViewBackgroundColor?.ToPlatform();
                middleView.Frame = new CGRect(CGPoint.Empty, new CGSize(70, 70));
                if (image is not null)
                {
                    middleView.SetImage(image.Value, UIControlState.Normal);
                    middleView.Frame = new CGRect(CGPoint.Empty, image.Value.Size + new CGSize(30, 30));
                }

                middleView.AutoresizingMask = UIViewAutoresizing.FlexibleRightMargin |
                                              UIViewAutoresizing.FlexibleLeftMargin |
                                              UIViewAutoresizing.FlexibleBottomMargin;
                middleView.Layer.CornerRadius = middleView.Frame.Width / 2;
                middleView.Layer.BorderWidth = 4;
                middleView.Layer.BorderColor = UIColor.White.CGColor;
                middleView.Layer.MasksToBounds = false;

                middleView.TouchUpInside += delegate
                {
                    tabbar.CenterView_Tapped();
                };;
            }

            middleView.Center = new CGPoint(View.Bounds.GetMidX(), TabBar.Frame.Top + 4);

            View.AddSubview(middleView);
        }
    }
}