using SSW.Rewards.Mobile.Controls;
using Color = Android.Graphics.Color;

namespace SSW.Rewards.Mobile.Renderers;

using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;

internal class CustomShellItemRenderer(IShellContext context) : ShellItemRenderer(context)
{
	public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
	{
		var view = base.OnCreateView(inflater, container, savedInstanceState);
		if (Context is not null && ShellItem is CustomTabBar { CenterViewVisible: true } tabBar)
		{
			var rootLayout = new FrameLayout(Context)
			{
				LayoutParameters =
					new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
			};

			rootLayout.AddView(view);
			const int middleViewSize = 280;
			var middleViewLayoutParams = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
																	  ViewGroup.LayoutParams.WrapContent,
																	  GravityFlags.CenterHorizontal |
																	  GravityFlags.Bottom)
			{
				BottomMargin = 30,
				Width = middleViewSize,
				Height = middleViewSize
			};
			var middleView = new Button(Context)
			{
				LayoutParameters = middleViewLayoutParams
			};
			middleView.Click += delegate
            {
                tabBar.CenterView_Tapped();
            };
			middleView.SetPadding(0, 0, 0, 0);
			if (tabBar.CenterViewBackgroundColor is not null)
			{
				var backgroundView = new View(Context)
				{
					LayoutParameters = middleViewLayoutParams
				};
				var backgroundDrawable = new GradientDrawable();
				backgroundDrawable.SetShape(ShapeType.Rectangle);
                backgroundDrawable.SetStroke(14, Color.White);
				backgroundDrawable.SetCornerRadius(middleViewSize / 2f);
				backgroundDrawable.SetColor(tabBar.CenterViewBackgroundColor.ToPlatform(Colors.Transparent));
				backgroundView.SetBackground(backgroundDrawable);
				rootLayout.AddView(backgroundView);
			}

			var context = tabBar.Window?.Page?.Handler?.MauiContext ?? Application.Current?.Windows.LastOrDefault()?.Page?.Handler?.MauiContext;
			tabBar.CenterViewImageSource?.LoadImage(context!, result =>
			{
				if (result?.Value is not BitmapDrawable drawable || drawable.Bitmap is null)
				{
					return;
				}
                
				middleView.LayoutParameters = new FrameLayout.LayoutParams(
					180, 180,
					GravityFlags.CenterHorizontal | GravityFlags.Bottom)
				{
					BottomMargin = middleViewLayoutParams.BottomMargin + 50
				};
				middleView.SetBackground(drawable);
				middleView.SetMinimumHeight(0);
				middleView.SetMinimumWidth(0);
			});

			rootLayout.AddView(middleView);
			return rootLayout;
		}

		return view;
	}
}