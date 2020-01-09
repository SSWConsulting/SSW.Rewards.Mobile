using System;
using System.Collections.Generic;

namespace SSW.Rewards.Application.User.Queries.GetUser
{
    public class UserViewModel
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Avatar { get; set; }

        public int Points { get; set; }
    }
}
