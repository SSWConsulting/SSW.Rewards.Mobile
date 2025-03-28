using Microsoft.Maui.Controls.Platform.Compatibility;

namespace SSW.Rewards.Mobile.Renderers;

public class CustomShellPageRendererTracker : ShellPageRendererTracker
{
    public CustomShellPageRendererTracker(IShellContext context)
        : base(context)
    {

    }

    protected override void UpdateTabBarVisible()
    {
        // Fixes MAUI bug where this can return a NullReferenceException
        // TODO: Remove when (hopefully) fixed in .NET MAUI 9
        // See: https://github.com/dotnet/maui/pull/26786
        if (ViewController is null || Page is null)
        {
            return;
        }
        
        base.UpdateTabBarVisible();
    }
}