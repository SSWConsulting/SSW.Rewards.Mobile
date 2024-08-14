using BarcodeScanning;
using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using Mopups.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ZXing.Net.Maui.Controls;
using SSW.Rewards.Mobile.Renderers;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SSW.Rewards.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            fonts.AddFont("FluentSystemIcons-Regular.ttf", "FluentIcons");
            fonts.AddFont("FA6Brands-Regular.otf", "FA6Brands");
            fonts.AddFont("FontAwesome6-Regular.otf", "FA6Regular");
            fonts.AddFont("FontAwesome6-Solid.otf", "FA6Solid");
            fonts.AddFont("Helvetica-Bold-Font.ttf", "HelveticaBold");
            fonts.AddFont("LiberationSans-Regular.ttf", "LiberationSansRegular");
            fonts.AddFont("LiberationSans-Bold.ttf", "LiberationSansBold");
            fonts.AddFont("LiberationSans-Italic.ttf", "LiberationSansItalic");
            fonts.AddFont("LiberationSans-BoldItalic.ttf", "LiberationSansBoldItalic");
        })
        .UseMauiCommunityToolkit()
        .UseFFImageLoading()
        .ConfigureMopups()
        .UseSkiaSharp()
        .UsePageResolver()
        .UseBarcodeReader()
        .UseBarcodeScanning()
        .RegisterFirebase()
        .ConfigureMauiHandlers((handlers) =>
        {
            handlers.AddHandler(typeof(TableView), typeof(CustomTableViewRenderer));
        });

        AppCenter.Start($"android={Constants.AppCenterAndroidId};" +
                  $"ios={Constants.AppCenterIOSId};",
                  typeof(Analytics), typeof(Crashes));

        builder.Services.AddDependencies();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        App.SetScope(builder.Services);
        
#if ANDROID
        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping(nameof(Editor), (handler, editor) =>
        {
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
        });

        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, entry) =>
        {
            // color in the underline while preserving the background color
            var e = (Entry)entry;
            if (e?.BackgroundColor == null)
            {
                return;
            }

            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(e.BackgroundColor.ToPlatform());
        });
#endif

#if IOS
        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping(nameof(Editor), (handler, editor) =>
        {
            handler.PlatformView.TintColor = UIKit.UIColor.FromRGB(204,65,65);
            handler.PlatformView.SmartDashesType = UIKit.UITextSmartDashesType.No;
            handler.PlatformView.SmartQuotesType = UIKit.UITextSmartQuotesType.No;
        });

        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, editor) =>
        {
            handler.PlatformView.TintColor = UIKit.UIColor.FromRGB(204,65,65);
        });
#endif

        return builder.Build();
    }
    
    private static MauiAppBuilder RegisterFirebase(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events =>
        {
#if IOS
            events.AddiOS(iOS => iOS.FinishedLaunching((app, launchOptions) => {
                Firebase.Core.App.Configure();
                return false;
            }));
#else
            events.AddAndroid(android => android.OnCreate((activity, bundle) => {
                Firebase.FirebaseApp.InitializeApp(activity);
            }));
#endif
        });

        return builder;
    }
}