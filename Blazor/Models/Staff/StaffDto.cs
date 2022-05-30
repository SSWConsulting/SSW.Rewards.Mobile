namespace SSW.Rewards.Admin.Models.Staff
{
    public class StaffDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Profile { get; set; }
        public string ProfilePhoto { get; set; }
        public bool IsDeleted { get; set; }
        public string TwitterUsername { get; set; }
        public string GitHubUsername { get; set; }
        public string LinkedInUrl { get; set; }
        public bool IsExternal { get; set; }
        public IEnumerable<string> Skills { get; set; } = Enumerable.Empty<string>();
    }
}
