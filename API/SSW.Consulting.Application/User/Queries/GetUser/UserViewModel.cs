using System;
using System.Collections.Generic;

namespace SSW.Consulting.Application.User.Queries.GetUser
{
    public class UserViewModel
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public string Picture { get; set; }
    }
}
