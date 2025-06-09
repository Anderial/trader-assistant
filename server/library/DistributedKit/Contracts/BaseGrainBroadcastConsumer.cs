using DistributedKit.Helper;
using Microsoft.Extensions.Logging;
using Orleans.BroadcastChannel;
using System.Diagnostics;

namespace DistributedKit.Contracts;

[GrainType($"{nameof(BaseGrainBroadcastConsumer<TMessage>)}`1")]
public abstract class BaseGrainBroadcastConsumer<TMessage>(ILogger<BaseGrainBroadcastConsumer<TMessage>> logger)
    : Grain, IIncomingGrainCallFilter, IOnBroadcastChannelSubscribed, IGrainWithGuidKey
    where TMessage : class
{
    private readonly string MessageType = typeof(TMessage).Name;

    private async Task BaseExecute(MessageContext<TMessage> messageContext)
    {
        var durationProcess = Stopwatch.StartNew();
        MessagingMetrics.MessageProcessingStarted();
        try
        {
            logger.LogTrace("Grain start process message {GrainId} {MessageType} {@Message}", this.GetPrimaryKeyString(), MessageType, messageContext.Message);
            await Execute(messageContext.Message);
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

    protected abstract Task Execute(TMessage message);

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var guid = this.GetPrimaryKey();

        logger.LogTrace("Start grain activate: {MessageType}, {GrainId}", MessageType, guid);

        return Task.CompletedTask;
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

    public Task OnSubscribed(IBroadcastChannelSubscription streamSubscription)
    {
        // todo: Eliminate 'MessageContext' after separating interfaces for subscribers
        streamSubscription.Attach<MessageContext<TMessage>>(BaseExecute, OnError);
        return Task.CompletedTask;
    }

    public async Task Invoke(IIncomingGrainCallContext context)
        => await IncomingGrainCallContextHelper.Invoke<TMessage>(nameof(Execute), MessageType, this.GetPrimaryKeyString(), context, logger);
}