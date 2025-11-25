using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using SSW.Rewards.Mobile.Controls;

namespace SSW.Rewards.Mobile.Renderers;

internal class CustomShellHandler : ShellRenderer
{
    protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
    {
        if (shellItem is CustomTabBar { CenterViewVisible: true })
        {
            return new CustomShellItemRenderer(this);
        }

        return base.CreateShellItemRenderer(shellItem);
    }

    protected override IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection)
    {
        if (shellSection.Parent?.Parent is CustomTabBar { CenterViewVisible: true })
        {
            return new CustomShellSectionRenderer(this);
        }

        return base.CreateShellSectionRenderer(shellSection);
    }
}