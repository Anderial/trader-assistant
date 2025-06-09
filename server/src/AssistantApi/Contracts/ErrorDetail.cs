namespace AssistantApi.Contracts;

public record ErrorDetail<TErrorCode> where TErrorCode : Enum
{
    public required TErrorCode ErrorCode { get; set; }
    public string? Message { get; set; }
}

public static class EnumExtensions
{
    public static string ToReadableString(this Enum value)
    {
        return value.ToString().Replace("_", " ");
    }
}