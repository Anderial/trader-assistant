using DistributedKit.Constants;
using DistributedKit.Contracts;
using Microsoft.Extensions.Logging;
using Orleans.Streams;

namespace DistributedKit.StreamHandler;

internal interface IPersistentStreamHandlerRunner
{
    string GetMessageTypeName();
    Task Start(Guid streamId);
    Task Stop();
}

internal interface IPersistentStreamHandlerRunner<TMessage> : IPersistentStreamHandlerRunner
{

}

internal sealed class PersistentStreamHandlerRunner<TMessage>(ILogger<PersistentStreamHandlerRunner<TMessage>> logger
    , IClusterClient clusterClient
    , IPersistentStreamHandlerWithGuidKey<TMessage> persistentStreamHandlerWithGuidKey
    ) : IPersistentStreamHandlerRunner<TMessage>
{
    private StreamSubscriptionHandle<MessageContext<TMessage>>? StreamSubscriptionHandle;
    private readonly string MessageTypeName = typeof(TMessage).Name;

    public string GetMessageTypeName() => MessageTypeName;

    public async Task Start(Guid streamId)
    {
        StreamSubscriptionHandle = await clusterClient
            .GetStreamProvider(StreamConstants.InMemoryStreamProvider)
            .GetStream<MessageContext<TMessage>>(MessageTypeName, streamId)
            .SubscribeAsync(async (message, y) =>
            {
                try
                {
                    logger.LogTrace("Start process message: {StreamId} : {MessageType} : {@Message}", streamId, MessageTypeName, message);
                    await persistentStreamHandlerWithGuidKey.Execute(message);
                    logger.LogTrace("Finish process message: {StreamId} : {MessageType} : {@Message}", streamId, MessageTypeName, message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Exception process message: {StreamId} : {MessageType} : {@Message}", streamId, MessageTypeName, message);
                    throw;
                }
            });
    }

    public async Task Stop()
    {
        if (StreamSubscriptionHandle != null)
            await StreamSubscriptionHandle.UnsubscribeAsync();
    }
}