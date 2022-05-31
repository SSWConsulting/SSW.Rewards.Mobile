namespace SSW.Rewards.Application.Users.Queries.GetCurrentUser
{
    public class CurrentUserViewModel
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public string ProfilePic { get; set; }

        public int Points { get; set; }

        public int Balance { get; set; }

        public string QRCode { get; set; }
    }
}
