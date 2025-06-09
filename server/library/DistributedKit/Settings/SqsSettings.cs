namespace DistributedKit.Settings;

public class SqsSettings
{
    public required string ServiceUri { get; set; }
    public string? SecretKey { get; set; } = null;
    public string? AccessKey { get; set; } = null;

    internal string GetConnectionString()
    {
        if (string.IsNullOrEmpty(SecretKey) && string.IsNullOrEmpty(AccessKey))
        {
            return $"Service={ServiceUri}";
        }
        else
        {
            return string
                .Join(";"
                    , $"Service={ServiceUri}"
                    , $"SecretKey={SecretKey}"
                    , $"AccessKey={AccessKey}"
                );
        }
    }
}
