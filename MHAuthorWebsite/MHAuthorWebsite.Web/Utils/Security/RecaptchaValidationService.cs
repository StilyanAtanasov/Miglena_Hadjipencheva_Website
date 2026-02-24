using MHAuthorWebsite.Core.Configuration.Security;
using MHAuthorWebsite.Web.Utils.Contracts;
using MHAuthorWebsite.Web.Utils.Enums;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace MHAuthorWebsite.Web.Utils.Security;

public sealed class RecaptchaValidationService : IRecaptchaValidationService
{
    private const string VerifyEndpoint = "https://www.google.com/recaptcha/api/siteverify";

    private readonly HttpClient _httpClient;
    private readonly RecaptchaSettings _settings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RecaptchaValidationService> _logger;

    public RecaptchaValidationService(
        HttpClient httpClient,
        IOptions<RecaptchaSettings> settings,
        IHttpContextAccessor httpContextAccessor,
        ILogger<RecaptchaValidationService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<RecaptchaValidationResult> VerifyV2Async(string? token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("reCAPTCHA v2 token is missing.");
            return RecaptchaValidationResult.Failure(RecaptchaFailureReason.MissingToken);
        }

        if (string.IsNullOrWhiteSpace(_settings.V2SecretKey))
        {
            _logger.LogError("reCAPTCHA v2 secret key is not configured.");
            return RecaptchaValidationResult.Failure(RecaptchaFailureReason.MissingSecret);
        }

        RecaptchaVerifyResponse? response = await VerifyAsync(_settings.V2SecretKey, token, cancellationToken);
        if (response is null) return RecaptchaValidationResult.Failure(RecaptchaFailureReason.RequestFailed);

        if (!response.Success)
        {
            _logger.LogWarning("reCAPTCHA v2 verification failed. Errors: {Errors}", string.Join(", ", response.ErrorCodes));
            return RecaptchaValidationResult.Failure(RecaptchaFailureReason.VerificationFailed);
        }

        return RecaptchaValidationResult.Success();
    }

    public async Task<RecaptchaValidationResult> VerifyV3Async(
        string? token,
        string expectedAction,
        double? minimumScore = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("reCAPTCHA v3 token is missing.");
            return RecaptchaValidationResult.Failure(RecaptchaFailureReason.MissingToken);
        }

        if (string.IsNullOrWhiteSpace(_settings.V3SecretKey))
        {
            _logger.LogError("reCAPTCHA v3 secret key is not configured.");
            return RecaptchaValidationResult.Failure(RecaptchaFailureReason.MissingSecret);
        }

        RecaptchaVerifyResponse? response = await VerifyAsync(_settings.V3SecretKey, token, cancellationToken);
        if (response is null) return RecaptchaValidationResult.Failure(RecaptchaFailureReason.RequestFailed);

        if (!response.Success)
        {
            _logger.LogWarning("reCAPTCHA v3 verification failed. Errors: {Errors}", string.Join(", ", response.ErrorCodes));
            return RecaptchaValidationResult.Failure(RecaptchaFailureReason.VerificationFailed);
        }

        if (!string.Equals(response.Action, expectedAction, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "reCAPTCHA v3 action mismatch. Expected: {ExpectedAction}. Received: {ReceivedAction}.",
                expectedAction,
                response.Action);
            return RecaptchaValidationResult.Failure(RecaptchaFailureReason.InvalidAction, response.Score);
        }

        double score = response.Score ?? 0;
        double threshold = minimumScore ?? _settings.V3MinimumScore;
        if (score < threshold)
        {
            _logger.LogWarning("reCAPTCHA v3 score too low. Score: {Score}. Threshold: {Threshold}.", score, threshold);
            return RecaptchaValidationResult.Failure(RecaptchaFailureReason.LowScore, score);
        }

        return RecaptchaValidationResult.Success(score);
    }

    private async Task<RecaptchaVerifyResponse?> VerifyAsync(
        string secret,
        string token,
        CancellationToken cancellationToken)
    {
        Dictionary<string, string> payload = new()
        {
            ["secret"] = secret,
            ["response"] = token
        };

        string? remoteIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrWhiteSpace(remoteIp)) payload["remoteip"] = remoteIp;

        using HttpContent content = new FormUrlEncodedContent(payload);

        try
        {
            using HttpResponseMessage response = await _httpClient.PostAsync(VerifyEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("reCAPTCHA verification endpoint returned status code {StatusCode}.", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<RecaptchaVerifyResponse>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify reCAPTCHA token.");
            return null;
        }
    }

    private sealed class RecaptchaVerifyResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("score")]
        public double? Score { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("error-codes")]
        public string[] ErrorCodes { get; set; } = Array.Empty<string>();
    }
}
