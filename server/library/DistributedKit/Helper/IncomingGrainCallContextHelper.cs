using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DistributedKit.Helper;

// todo: Search for something ready-made in Orleans for working with metrics, it might be possible to avoid this code.
// Or think about how to eliminate the duplication of metrics in two places!
// Specifically in: "Task Invoke(IIncomingGrainCallContext context)" and "Task BaseExecute(...)" methods.
// Currently this is done so that, when directly accessing a grain and when working with a grain through messages, we can get logs and processing metrics.

internal static class IncomingGrainCallContextHelper
{
    internal static async Task Invoke<TMessage>(string executeMethodName, string MessageType, string primaryKeyString, IIncomingGrainCallContext context, ILogger logger)
    {
        if (context.ImplementationMethod.Name != executeMethodName)
        {
            await context.Invoke();
            return;
        }

        var durationProcess = Stopwatch.StartNew();
        MessagingMetrics.MessageProcessingStarted();
        try
        {
            logger.LogTrace("Grain start process message {GrainId} {MessageType} {@Message}", primaryKeyString, MessageType, context.Request);
            await context.Invoke();
            MessagingMetrics.ObserveSuccessfullyOperationDuration(MessageType, durationProcess.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Grain error process message {GrainId} {MessageType} {ErrorMessage}", primaryKeyString, MessageType, ex.Message);
            MessagingMetrics.ObserveExceptionOperationDuration(MessageType, durationProcess.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            durationProcess.Stop();
            MessagingMetrics.MessageProcessingFinished();
        }
    }
}
