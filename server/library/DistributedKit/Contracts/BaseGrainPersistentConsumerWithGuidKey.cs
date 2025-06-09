using DistributedKit.Constants;
using DistributedKit.Helper;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using System.Diagnostics;

namespace DistributedKit.Contracts;

[UserKeyPlacement]
[GrainType($"{nameof(BaseGrainPersistentConsumerWithGuidKey<TMessage>)}`1")]
public abstract class BaseGrainPersistentConsumerWithGuidKey<TMessage>(ILogger<BaseGrainPersistentConsumerWithGuidKey<TMessage>> logger)
    : Grain, IIncomingGrainCallFilter, IGrainWithGuidKey
    where TMessage : class
{
    private readonly string MessageType = typeof(TMessage).Name;

    private async Task BaseExecute(MessageContext<TMessage> messageContext, CancellationToken cancellationToken)
    {
        var durationProcess = Stopwatch.StartNew();
        MessagingMetrics.MessageProcessingStarted();
        try
        {
            logger.LogTrace("Grain start process message {GrainId} {MessageType} {@Message}", this.GetPrimaryKeyString(), MessageType, messageContext);
            await Execute(messageContext, cancellationToken);
            MessagingMetrics.ObserveSuccessfullyOperationDuration(MessageType, durationProcess.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Grain error process message {GrainId} {MessageType} {ErrorMessage}", this.GetPrimaryKeyString(), MessageType, ex.Message);
            MessagingMetrics.ObserveExceptionOperationDuration(MessageType, durationProcess.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            durationProcess.Stop();
            MessagingMetrics.MessageProcessingFinished();
        }
    }

    protected abstract Task Execute(MessageContext<TMessage> messageContext, CancellationToken cancellationToken);

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var guid = this.GetPrimaryKey();

        logger.LogTrace("Start grain activate: {MessageType}, {GrainId}", MessageType, guid);

        var streamProvider = this.GetStreamProvider(StreamConstants.InMemoryStreamProvider);
        var stream = streamProvider.GetStream<MessageContext<TMessage>>(MessageType, guid);

        await stream.SubscribeAsync(
            async (messageContext, streamSequenceToken) =>
            {
                await BaseExecute(messageContext, cancellationToken);
            }
            , OnError
        );
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        logger.LogTrace("Grain deactivated {GrainId} {MessageType}", this.GetPrimaryKeyString(), MessageType);
        return Task.CompletedTask;
    }

    private Task OnError(Exception ex)
    {
        logger.LogError(ex, "Grain Error {GrainId} {MessageType}", this.GetPrimaryKeyString(), MessageType);
        return Task.CompletedTask;
    }

    public async Task Invoke(IIncomingGrainCallContext context)
        => await IncomingGrainCallContextHelper.Invoke<TMessage>(nameof(Execute), MessageType, this.GetPrimaryKeyString(), context, logger);
}