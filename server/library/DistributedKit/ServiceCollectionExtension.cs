using DistributedKit.Constants;
using DistributedKit.Settings;
using DistributedKit.StreamHandler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Orleans.Providers.MongoDB.Configuration;

namespace DistributedKit;

public static class ServiceCollectionExtension
{
    public static IHostBuilder UseDistributedKitService(this IHostBuilder hostBuilder
        , Action<IServiceCollection> actionServiceCollection
        , IConfigurationSection? configurationSection = null
        )
    {
        hostBuilder
            .UseOrleans((context, siloBuilder) =>
            {
                var config = configurationSection ?? context.Configuration;
                var clusterOptions = config.GetSection(nameof(OrleansClusterSettings)).Get<OrleansClusterSettings>()!;
                var sqsOptions = config.GetSection(nameof(SqsSettings)).Get<SqsSettings>()!;
                var podNamespace = Environment.GetEnvironmentVariable("POD_NAMESPACE");

                if (clusterOptions.UseKubernetesHosting)
                {
                    siloBuilder
                        .UseKubernetesHosting()
                        ;
                }
                else
                {
                    siloBuilder
                        .Configure<EndpointOptions>(options =>
                        {
                            options.SiloPort = clusterOptions.SiloPort;
                            options.GatewayPort = clusterOptions.GatewayPort;
                        })
                        ;
                }

                siloBuilder
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = string.IsNullOrWhiteSpace(podNamespace) ? clusterOptions.ClusterId : $"{podNamespace}-{clusterOptions.ClusterId}";
                        options.ServiceId = string.IsNullOrWhiteSpace(podNamespace) ? clusterOptions.ServiceId : $"{podNamespace}-{clusterOptions.ServiceId}";
                    })
                    .Configure<GrainCollectionOptions>(options =>
                    {
                        options.CollectionAge = TimeSpan.FromMinutes(clusterOptions.Cluster.GrainCollectionAgeFromMinutes);
                    })
                    .Configure<ClusterMembershipOptions>(options =>
                    {
                        options.NumMissedProbesLimit = clusterOptions.Cluster.NumMissedProbesLimit;
                        options.DeathVoteExpirationTimeout = TimeSpan.FromMilliseconds(clusterOptions.Cluster.DeathVoteExpirationTimeoutFromMilliseconds);
                        options.TableRefreshTimeout = TimeSpan.FromMilliseconds(clusterOptions.Cluster.TableRefreshTimeoutFromMilliseconds);
                        options.DefunctSiloCleanupPeriod = TimeSpan.FromMilliseconds(clusterOptions.Cluster.DefunctSiloCleanupPeriodFromMilliseconds);
                        options.DefunctSiloExpiration = TimeSpan.FromMilliseconds(clusterOptions.Cluster.DefunctSiloExpirationFromMilliseconds);
                        options.UseLivenessGossip = clusterOptions.Cluster.UseLivenessGossip;
                    })
                    .UseMongoDBClient(clusterOptions.MongoMembership.GetConnectionString())
                    .UseMongoDBClustering(options =>
                    {
                        options.DatabaseName = clusterOptions.MongoMembership.DatabaseName;
                        options.Strategy = clusterOptions.MongoMembership.Strategy switch
                        {
                            MembershipStrategy.SingleDocument => MongoDBMembershipStrategy.SingleDocument,
                            MembershipStrategy.Multiple => MongoDBMembershipStrategy.Multiple,
                            _ => throw new InvalidOperationException("Error parse MongoDBMembershipStrategy"),
                        };
                    })
                    .AddMemoryGrainStorage("PubSubStore")
                    .AddMemoryStreams(StreamConstants.InMemoryStreamProvider)
                    //.AddSqsStreams(StreamConstants.PersistentStreamProvider, options => options.ConnectionString = sqsOptions.GetConnectionString())
                    .AddBroadcastChannel(StreamConstants.BroadcastChannel)
                    .AddPlacementDirector<UserKeyPlacementStrategy, UserKeyPlacementDirector>()
                    ;
                siloBuilder
                    .ConfigureServices(serviceCollection =>
                    {
                        actionServiceCollection(serviceCollection);
                        serviceCollection.AddTransient<IProducerClient, ProducerClient>();
                    })
                    ;
            });

        return hostBuilder;
    }

    /// <summary>
    /// Configures DistributedKit client with stream subscription support
    /// </summary>
    /// <param name="hostBuilder">Host builder</param>
    /// <param name="configureHandlers">Action to configure stream handlers</param>
    /// <param name="configurationSection">Configuration section for Orleans (optional)</param>
    /// <returns>Host builder for chaining</returns>
    public static IHostBuilder UseDistributedKitClient(this IHostBuilder hostBuilder
        , string clientNamespace
        , Action<IHandlerRegistrator>? configureHandlers = null
        , IConfigurationSection? configurationSection = null
        )
    {
        hostBuilder
            .UseOrleansClient((context, clientBuilder) =>
            {
                var config = configurationSection ?? context.Configuration;
                var clusterOptions = config.GetSection(nameof(OrleansClientSettings)).Get<OrleansClientSettings>()!;
                var sqsOptions = config.GetSection(nameof(SqsSettings)).Get<SqsSettings>()!;
                var podNamespace = Environment.GetEnvironmentVariable("POD_NAMESPACE");

                clientBuilder
                    .UseConnectionRetryFilter(async (ex, cancellationToken) =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                        return true;
                    })
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = string.IsNullOrWhiteSpace(podNamespace) ? clusterOptions.ClusterId : $"{podNamespace}-{clusterOptions.ClusterId}";
                        options.ServiceId = string.IsNullOrWhiteSpace(podNamespace) ? clusterOptions.ServiceId : $"{podNamespace}-{clusterOptions.ServiceId}";
                    })
                    .UseMongoDBClient(clusterOptions.MongoMembership.GetConnectionString())
                    .UseMongoDBClustering(options =>
                    {
                        options.DatabaseName = clusterOptions.MongoMembership.DatabaseName;
                        options.Strategy = clusterOptions.MongoMembership.Strategy switch
                        {
                            MembershipStrategy.SingleDocument => MongoDBMembershipStrategy.SingleDocument,
                            MembershipStrategy.Multiple => MongoDBMembershipStrategy.Multiple,
                            _ => throw new InvalidOperationException("Error parse MongoDBMembershipStrategy"),
                        };
                    })
                    .AddMemoryStreams(StreamConstants.InMemoryStreamProvider)
                    //.AddSqsStreams(StreamConstants.PersistentStreamProvider, options => options.ConnectionString = sqsOptions.GetConnectionString())
                    .AddBroadcastChannel(StreamConstants.BroadcastChannel)
                    ;

            });

        hostBuilder.ConfigureServices(x =>
            {

                var optionsBuilder = x.AddOptions<OrleansClientSettings>();
                if (configurationSection != null)
                    optionsBuilder.Bind(configurationSection);
                else
                    optionsBuilder.BindConfiguration(nameof(OrleansClientSettings));

                x.PostConfigure<OrleansClientSettings>(x => x.UniqueClusterClientId = Guid.NewGuid());
                x.PostConfigure<OrleansClientSettings>(x => x.ClientNamespace = clientNamespace);

                x.AddTransient<IProducerClient, ProducerClient>();
                x.AddSingleton<IStreamSubscriptionManager, StreamSubscriptionManager>();
                HandlerRegistrator handlerRegistrator = new();
                configureHandlers?.Invoke(handlerRegistrator);
                foreach (var item in handlerRegistrator.Items)
                {
                    x.AddTransient(item.IHandlerType, item.HandlerType);
                    x.AddTransient(item.IHandlerRunnerType, item.HandlerRunnerType);
                }
            }
        );

        return hostBuilder;
    }
}
