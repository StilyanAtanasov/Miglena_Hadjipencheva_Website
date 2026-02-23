namespace MHAuthorWebsite.Web.Utils.Contracts;

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

public sealed class RecaptchaValidationResult
{
    private RecaptchaValidationResult(
        bool isSuccess,
        RecaptchaFailureReason failureReason,
        double? score)
    {
        IsSuccess = isSuccess;
        FailureReason = failureReason;
        Score = score;
    }

    public bool IsSuccess { get; }

    public RecaptchaFailureReason FailureReason { get; }

    public double? Score { get; }

    public bool RequiresManualChallenge => FailureReason == RecaptchaFailureReason.LowScore;

    public static RecaptchaValidationResult Success(double? score = null)
        => new(true, RecaptchaFailureReason.None, score);

    public static RecaptchaValidationResult Failure(RecaptchaFailureReason reason, double? score = null)
        => new(false, reason, score);
}
