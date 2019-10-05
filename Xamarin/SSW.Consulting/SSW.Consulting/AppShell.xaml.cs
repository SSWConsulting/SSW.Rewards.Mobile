using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Auth;
using SSW.Consulting.Views;
using Xamarin.Forms;

namespace SSW.Consulting
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public void Handle_LogOutClicked(object sender, EventArgs e)
        {
            Auth.SignOut();
        }

        public void Handle_QuizClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Handle_EventsClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Handle_JoinClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Handle_AboutClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Handle_HowToPlayClicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new OnBoarding());
        }

        public AppShell()
        {
            InitializeComponent();
        }
    }
}
