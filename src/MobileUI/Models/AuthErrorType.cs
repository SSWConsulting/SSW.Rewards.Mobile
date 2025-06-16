namespace SSW.Rewards.Models;

public enum AuthErrorType
{
    None,
    Cancelled,
    InvalidToken,
    Exception,
    SilentLoginFailed,
    UnknownError
}