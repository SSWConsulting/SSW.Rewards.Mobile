using System;
using Contacts;
using Foundation;
using SSW.Consulting.Services;

namespace SSW.Consulting.iOS
{
    public class Contacts : IContacts
    {
        public Contacts()
        {
        }

        public void SaveContact(string Name, string Number, string Email)
        {
            var store = new CNContactStore();
            var contact = new CNMutableContact();
            var cellPhone = new CNLabeledValue<CNPhoneNumber>(CNLabelPhoneNumberKey.Mobile, new CNPhoneNumber(Number));
            var emailAddress = new CNLabeledValue<NSString>(CNLabelKey.Work, new NSString(Email));
            var phoneNumber = new[] { cellPhone };
            contact.PhoneNumbers = phoneNumber;
            contact.GivenName = Name;
            contact.EmailAddresses = new CNLabeledValue<NSString>[] { emailAddress };
            var saveRequest = new CNSaveRequest();
            saveRequest.AddContact(contact, store.DefaultContainerIdentifier);
        }
    }
}
