namespace MHAuthorWebsite.Web.Utils.Enums;

public enum RecaptchaFailureReason
{
    None = 0,
    MissingToken = 1,
    MissingSecret = 2,
    RequestFailed = 3,
    VerificationFailed = 4,
    InvalidAction = 5,
    LowScore = 6
}