using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using ImageCircle.Forms.Plugin.iOS;
using PanCardView.iOS;
using SSW.Consulting.Services;
using UIKit;
using Xamarin.Forms;

namespace SSW.Consulting.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.SetFlags("Shell_Experimental", "Visual_Experimental", "CollectionView_Experimental", "FastRenderers_Experimental");

            Rg.Plugins.Popup.Popup.Init();

            AppCenter.Start("e33283b1-7326-447d-baae-e783ece0789b",
                  typeof(Auth), typeof(Analytics), typeof(Crashes));

            global::Xamarin.Forms.Forms.Init();

            ZXing.Net.Mobile.Forms.iOS.Platform.Init();
            CardsViewRenderer.Preserve();
            ImageCircleRenderer.Init();
            Bootstrapper.Init();

            DependencyService.Register<IContacts, Contacts>();

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
        /*
        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
            return true;
        }*/
    }
}
