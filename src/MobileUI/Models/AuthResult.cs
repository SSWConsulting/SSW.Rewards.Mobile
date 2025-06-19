namespace SSW.Rewards.Models;

public class AuthResult
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string IdentityToken { get; set; }
    public DateTimeOffset AccessTokenExpiration { get; set; }
    public bool IsSuccess => !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(IdentityToken) && Error == AuthErrorType.None;
    public AuthErrorType Error { get; set; } = AuthErrorType.None;
    public string ErrorDescription { get; set; }
}