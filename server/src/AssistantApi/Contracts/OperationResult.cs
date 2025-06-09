using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AssistantApi.Contracts;

public record OperationResult<TResponse, TErrorCode> where TErrorCode : Enum
{
    [MemberNotNullWhen(true, nameof(ResponseObject))]
    [MemberNotNullWhen(false, nameof(Errors))]
    [JsonInclude]
    public bool Successful { get; private set; }

    [JsonInclude]
    public TResponse? ResponseObject { get; private set; }

    [JsonInclude]
    public IReadOnlyCollection<ErrorDetail<TErrorCode>>? Errors { get; private set; }

    /// <summary>
    /// Success operation
    /// </summary>
    public static OperationResult<TResponse, TErrorCode> Success(TResponse responseObject)
        => new() { Successful = true, ResponseObject = responseObject };

    /// <summary>
    /// Failed operation
    /// Don't use this method to hide exceptions.
    /// Use this method to handle logic without creating new exceptions.
    /// </summary>
    public static OperationResult<TResponse, TErrorCode> Failed(params ErrorDetail<TErrorCode>[] errors)
        => new() { Successful = false, Errors = errors };

    /// <summary>
    /// Failed operation
    /// Don't use this method to hide exceptions.
    /// Use this method to handle logic without creating new exceptions.
    /// </summary>
    public static OperationResult<TResponse, TErrorCode> Failed(params TErrorCode[] codes)
        => new() { Successful = false, Errors = codes.Select(x => new ErrorDetail<TErrorCode>() { ErrorCode = x }).ToArray() };

    /// <summary>
    /// Failed operation
    /// Don't use this method to hide exceptions.
    /// Use this method to handle logic without creating new exceptions.
    /// </summary>
    public static OperationResult<TResponse, TErrorCode> Failed(bool setAutoMessage = false, params TErrorCode[] codes)
        => new() { Successful = false, Errors = codes.Select(x => new ErrorDetail<TErrorCode>() { ErrorCode = x, Message = setAutoMessage ? x.ToReadableString() : null }).ToArray() };

    /// <summary>
    /// Failed operation
    /// Don't use this method to hide exceptions.
    /// Use this method to handle logic without creating new exceptions.
    /// </summary>
    public static OperationResult<TResponse, TErrorCode> Failed(params (TErrorCode errorCode, string message)[] errors)
        => new() { Successful = false, Errors = errors.Select(x => new ErrorDetail<TErrorCode>() { ErrorCode = x.errorCode, Message = x.message }).ToArray() };
}

public record OperationResult<TErrorCode> where TErrorCode : Enum
{
    [MemberNotNullWhen(false, nameof(Errors))]
    public bool Successful { get; private set; }

    public IReadOnlyCollection<ErrorDetail<TErrorCode>>? Errors { get; private set; }

    /// <summary>
    /// Success operation
    /// </summary>
    public static OperationResult<TErrorCode> Success()
        => new() { Successful = true };

    /// <summary>
    /// Failed operation
    /// Don't use this method to hide exceptions.
    /// Use this method to handle logic without creating new exceptions.
    /// </summary>
    public static OperationResult<TErrorCode> Failed(params ErrorDetail<TErrorCode>[] errors)
        => new() { Successful = false, Errors = errors };

    /// <summary>
    /// Failed operation
    /// Don't use this method to hide exceptions.
    /// Use this method to handle logic without creating new exceptions.
    /// </summary>
    public static OperationResult<TErrorCode> Failed(params TErrorCode[] codes)
        => new() { Successful = false, Errors = codes.Select(x => new ErrorDetail<TErrorCode>() { ErrorCode = x }).ToArray() };

    /// <summary>
    /// Failed operation
    /// Don't use this method to hide exceptions.
    /// Use this method to handle logic without creating new exceptions.
    /// </summary>
    public static OperationResult<TErrorCode> Failed(bool setAutoMessage = false, params TErrorCode[] codes)
        => new() { Successful = false, Errors = codes.Select(x => new ErrorDetail<TErrorCode>() { ErrorCode = x, Message = setAutoMessage ? x.ToReadableString() : null }).ToArray() };

    /// <summary>
    /// Failed operation
    /// Don't use this method to hide exceptions.
    /// Use this method to handle logic without creating new exceptions.
    /// </summary>
    public static OperationResult<TErrorCode> Failed(params (TErrorCode errorCode, string message)[] errors)
        => new() { Successful = false, Errors = errors.Select(x => new ErrorDetail<TErrorCode>() { ErrorCode = x.errorCode, Message = x.message }).ToArray() };
}