using BarcodeScanning;
using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Mopups.Hosting;
using Plugin.Firebase.Crashlytics;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SSW.Rewards.Mobile.Renderers;

#if IOS
using Plugin.Firebase.Core.Platforms.iOS;
using Plugin.Firebase.CloudMessaging;
using Foundation;
#elif ANDROID
using Microsoft.Maui.Platform;
using Plugin.Firebase.Analytics;
using Plugin.Firebase.Core.Platforms.Android;
#endif

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
            fonts.AddFont("FluentSystemIcons-Filled.ttf", "FluentIconsFilled");
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
        .UseBarcodeScanning()
        .RegisterFirebase()
        .RegisterUrlHandling()
        .ConfigureMauiHandlers((handlers) =>
        {
            handlers.AddHandler(typeof(TableView), typeof(CustomTableViewRenderer));
            handlers.AddHandler<Border, NotAnimatedBorderHandler>();
            handlers.AddHandler(typeof(Shell), typeof(CustomShellHandler));
        });

        builder.Services.AddDependencies();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        // Log all unhandled exceptions
        MauiExceptions.UnhandledException += (_, args) =>
        {
            CrossFirebaseCrashlytics.Current.RecordException(args.ExceptionObject as Exception);
        };
        
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
            events.AddiOS(iOS => iOS.WillFinishLaunching((app, launchOptions) => {
                CrossFirebase.Initialize();
                FirebaseCloudMessagingImplementation.Initialize();
                CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(true);
                return false;
            }));
#elif ANDROID
            events.AddAndroid(android => android.OnCreate((activity, bundle) => {
                CrossFirebase.Initialize(activity);
                FirebaseAnalyticsImplementation.Initialize(activity);
                CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(true);
            }));
#endif
        });

        return builder;
    }

    private static MauiAppBuilder RegisterUrlHandling(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events =>
        {
#if IOS
            events.AddiOS(ios =>
            {
                ios.OpenUrl((app, url, options) => HandleAppLink(url.AbsoluteString));
                
                ios.FinishedLaunching((app, data)
                    => HandleAppLink(app.UserActivity));

                ios.ContinueUserActivity((app, userActivity, handler)
                    => HandleAppLink(userActivity));

                if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
                {
                    ios.SceneWillConnect((scene, sceneSession, sceneConnectionOptions)
                        => HandleAppLink(sceneConnectionOptions.UserActivities.ToArray()
                            .FirstOrDefault(a => a.ActivityType == NSUserActivityType.BrowsingWeb)));

                    ios.SceneContinueUserActivity((scene, userActivity)
                        => HandleAppLink(userActivity));
                }
            });
#elif ANDROID
            events.AddAndroid(android => 
            {
                android.OnCreate((activity, bundle) =>
                {
                    var action = activity.Intent?.Action;
                    var data = activity.Intent?.Data?.ToString();

                    if (action != Android.Content.Intent.ActionView || data is null)
                    {
                        return;
                    }

                    Task.Run(() => HandleAppLink(data));
                });

                android.OnNewIntent((activity, intent) =>
                {
                    if (intent != null)
                    {
                        var action = intent.Action;
                        var data = intent.Data?.ToString();
                        
                        if (action != Android.Content.Intent.ActionView || data is null)
                        {
                            return;
                        }

                        Task.Run(() => HandleAppLink(data));
                    }
                });
            });
#endif
        });

        return builder;
    }

#if IOS || MACCATALYST
    static bool HandleAppLink(NSUserActivity? userActivity)
    {
        if (userActivity is null || userActivity.ActivityType != NSUserActivityType.BrowsingWeb ||
            userActivity.WebPageUrl is null)
        {
            return false;
        }

        return HandleAppLink(userActivity.WebPageUrl.ToString());
    }
#endif

    static bool HandleAppLink(string url)
    {
        if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
        {
            return false;
        }

        App.Current?.SendOnAppLinkRequestReceived(uri);
        return true;

    }
}