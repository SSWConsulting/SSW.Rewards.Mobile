using System;
using System.Collections.Generic;

namespace SSW.Consulting.Application.User.Queries.GetCurrentUser
{
    public class CurrentUserViewModel
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public string Picture { get; set; }

        public int Points { get; set; }
    }
}
