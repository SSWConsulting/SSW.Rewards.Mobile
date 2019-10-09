using System;
using System.Collections.Generic;

namespace SSW.Consulting.Models
{
    public class DevProfile
    {
        public int id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string Bio { get; set; }
        public string Picture { get; set; }
        public string TwitterID { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<string> Skills { get; set; }
    }
}
