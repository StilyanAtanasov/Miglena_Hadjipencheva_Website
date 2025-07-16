namespace MHAuthorWebsite.Core.Common.Utils;

public class ServiceResult
{
    public bool Success { get; set; } = true;

    public bool HasPermission { get; set; } = true;

    public bool Found { get; set; } = true;

    public bool IsBadRequest { get; set; } = false;

    public Dictionary<string, string> Errors { get; set; } = new(); // Key -> propertyName | Value -> customErrorMessage

    public static ServiceResult Ok() => new();

    public static ServiceResult Failure(Dictionary<string, string>? errors = null) => new()
    {
        Success = false,
        Errors = errors ?? new()
    };

    public static ServiceResult NotFound() => new()
    {
        Success = false,
        Found = false
    };

    public static ServiceResult Forbidden() => new()
    {
        Success = false,
        HasPermission = false
    };

    public static ServiceResult BadRequest(Dictionary<string, string>? errors = null) => new()
    {
        Success = false,
        IsBadRequest = true,
        Errors = errors ?? new()
    };
}

public class ServiceResult<TResult> : ServiceResult
{
    public TResult? Result { get; set; }

    public bool HasResult() => Success && Result is not null;

    public static ServiceResult<TResult> Ok(TResult result) => new()
    {
        Result = result,
        Success = true
    };

    public new static ServiceResult<TResult> Failure(Dictionary<string, string>? errors = null) => new()
    {
        Success = false,
        Errors = errors ?? new()
    };

    public new static ServiceResult<TResult> NotFound() => new()
    {
        Success = false,
        Found = false
    };

    public new static ServiceResult<TResult> Forbidden() => new()
    {
        Success = false,
        HasPermission = false
    };

    public new static ServiceResult<TResult> BadRequest(Dictionary<string, string>? errors = null) => new()
    {
        Success = false,
        IsBadRequest = true,
        Errors = errors ?? new()
    };
}