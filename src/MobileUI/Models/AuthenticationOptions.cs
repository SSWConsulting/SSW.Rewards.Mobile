namespace SSW.Rewards.Models;

public class AuthenticationOptions
{
    public string Authority { get; set; }
    public string ClientId { get; set; }
    public string Scope { get; set; }
    public string RedirectUri { get; set; }
    public TimeSpan TokenRefreshBuffer { get; set; } = TimeSpan.FromMinutes(2);
}
