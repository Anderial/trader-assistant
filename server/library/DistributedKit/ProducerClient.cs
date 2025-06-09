using DistributedKit.Constants;
using DistributedKit.Contracts;
using Microsoft.Extensions.Logging;
using Orleans.BroadcastChannel;

namespace DistributedKit;

public interface IProducerClient
{
    Task Send<TMessage>(TMessage message, Guid accountId, string? messageId = null) where TMessage : class;
    Task SendBroadcastChannelMessage<TMessage>(TMessage message, string? messageId = null) where TMessage : class;
    TGrain GetGrain<TGrain>(Guid grainKey) where TGrain : IGrainWithGuidKey;
}

internal sealed class ProducerClient(IClusterClient clusterClient, ILogger<ProducerClient> logger) : IProducerClient
{
    public async Task Send<TMessage>(TMessage message, Guid accountId, string? messageId = null) where TMessage : class
    {
        var messageType = typeof(TMessage).Name;
        var stream = clusterClient
            .GetStreamProvider(StreamConstants.InMemoryStreamProvider)
            .GetStream<MessageContext<TMessage>>(StreamId.Create(messageType, accountId));
        await stream.OnNextAsync(new MessageContext<TMessage>(accountId, messageId ?? string.Empty, message));
        logger.LogTrace("Message published: {MessageType} : {AccountId} : {MessageId} : {@Message}", messageType, accountId, messageId, message);
    }

    public async Task SendBroadcastChannelMessage<TMessage>(TMessage message, string? messageId = null) where TMessage : class
    {
        var messageType = typeof(TMessage).Name;
        var stream = clusterClient
            .GetBroadcastChannelProvider(StreamConstants.BroadcastChannel)
            .GetChannelWriter<MessageContext<TMessage>>(ChannelId.Create(messageType, Guid.Empty));
        // todo: Eliminate 'MessageContext' after separating interfaces for subscribers
        await stream.Publish(new MessageContext<TMessage>(Guid.Empty, string.Empty, message));
        logger.LogTrace("Message published: {MessageType} : {MessageId} : {@Message}", messageType, messageId, message);
    }

    public TGrain GetGrain<TGrain>(Guid grainKey) where TGrain : IGrainWithGuidKey
    {
        return clusterClient.GetGrain<TGrain>(grainKey);
    }
}
