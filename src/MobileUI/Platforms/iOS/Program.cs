using IdentityModel.Client;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using System;
using System.Runtime.Serialization;
using System.Text;
using UIKit;

namespace SSW.Rewards.Mobile;

public class Program
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
		UIApplication.Main(args, null, typeof(AppDelegate));
	}
}
