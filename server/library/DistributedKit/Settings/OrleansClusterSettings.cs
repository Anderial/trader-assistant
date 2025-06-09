namespace DistributedKit.Settings;

public class OrleansClusterSettings
{
    public required string ClusterId { get; set; }
    public required string ServiceId { get; set; }
    public int SiloPort { get; set; }
    public int GatewayPort { get; set; }
    public required MongoMembership MongoMembership { get; set; }
    public required Cluster Cluster { get; set; }
    public required bool UseKubernetesHosting { get; set; }
}

public class Cluster
{
    public int NumMissedProbesLimit { get; set; }
    public int DeathVoteExpirationTimeoutFromMilliseconds { get; set; }
    public int TableRefreshTimeoutFromMilliseconds { get; set; }
    public int DefunctSiloCleanupPeriodFromMilliseconds { get; set; }
    public int DefunctSiloExpirationFromMilliseconds { get; set; }
    public bool UseLivenessGossip { get; set; }
    public int GrainCollectionAgeFromMinutes { get; set; }
}

public class MongoMembership
{
    public required string ServiceUri { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required int ConnectTimeoutMS { get; set; }
    public required string DatabaseName { get; set; }
    public required bool Tls { get; set; }
    public required MembershipStrategy Strategy { get; set; }

    internal string GetConnectionString()
    {
        return $"mongodb://{Username}:{Password}@{ServiceUri}/?tls={Tls}&tlsInsecure={Tls}&connectTimeoutMS={ConnectTimeoutMS}&retryWrites=false";
    }
}

public enum MembershipStrategy
{
    SingleDocument,
    Multiple
}