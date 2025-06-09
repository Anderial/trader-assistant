namespace DistributedKit.Settings;

public class OrleansClientSettings
{
    public required string ClusterId { get; set; }
    public required string ServiceId { get; set; }
    public required MongoMembership MongoMembership { get; set; }
    public required Guid UniqueClusterClientId { get; set; }
    public required string ClientNamespace { get; set; }
}
