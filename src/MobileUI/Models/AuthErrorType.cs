namespace SSW.Rewards.Models;

public enum AuthErrorType
{
    None,
    Cancelled,
    AccessDenied,
    InvalidToken,
    InvalidRequest,
    InvalidGrant,
    Exception,
    SilentLoginFailed,
    NetworkError,
    ServerError,
    UnknownError
}