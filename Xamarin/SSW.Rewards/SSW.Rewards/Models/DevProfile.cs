using System.Collections.Generic;

namespace SSW.Rewards.Models
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

        public string GitHubID { get; set; }

        public string LinkedInId { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public bool IsExternal { get; set; }

        public List<StaffSkillDto> Skills { get; set; }
    }
}
