using System;
namespace SSW.Consulting.Services
{
    public interface IContacts
    {
        void SaveContact(string Name, string Number, string Email);
    }
}
