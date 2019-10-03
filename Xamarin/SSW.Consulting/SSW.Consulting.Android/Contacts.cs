using System;
using Android.App;
using Android.Content;
using Android.Provider;
using Android.Widget;
using SSW.Consulting.Services;
using Xamarin.Forms;

namespace SSW.Consulting.Droid
{
    public class Contacts : IContacts
    {
        public Contacts()
        {
        }

        public void SaveContact(string Name, string Number, string Email)
		{

			var activity = Forms.Context as Activity;
			var intent = new Intent(Intent.ActionInsert);
			intent.SetType(ContactsContract.Contacts.ContentType);
			intent.PutExtra(ContactsContract.Intents.Insert.Name, Name);
			intent.PutExtra(ContactsContract.Intents.Insert.Phone, Number);
            intent.PutExtra(ContactsContract.Intents.Insert.Email, Email);
			activity.StartActivity(intent);
			Toast.MakeText(activity, "ContactSaved", ToastLength.Short).Show();
		}
	}
}
