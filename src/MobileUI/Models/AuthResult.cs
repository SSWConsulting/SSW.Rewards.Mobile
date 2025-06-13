namespace SSW.Rewards.Models;

public class AuthResult
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string IdentityToken { get; set; }
    public DateTimeOffset AccessTokenExpiration { get; set; }
    public bool IsSuccess => !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(IdentityToken);
    public string Error { get; set; }
    public string ErrorDescription { get; set; }
}
