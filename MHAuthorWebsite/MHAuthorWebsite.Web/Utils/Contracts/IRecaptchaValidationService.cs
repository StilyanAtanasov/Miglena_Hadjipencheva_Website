namespace MHAuthorWebsite.Web.Utils.Contracts;

public interface IRecaptchaValidationService
{
    Task<RecaptchaValidationResult> VerifyV2Async(string? token, CancellationToken cancellationToken = default);

    Task<RecaptchaValidationResult> VerifyV3Async(
        string? token,
        string expectedAction,
        double? minimumScore = null,
        CancellationToken cancellationToken = default);
}
