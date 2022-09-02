namespace SSW.Rewards.Models
{
    public class SocialUsernameMessage
    {
        public const string SocialUsernameAddedMessage = "SocialUsernameAdded";

        public string Username { get; set; }

        public string PlatformName { get; set; }
    }
}
