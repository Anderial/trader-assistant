using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace DistributedKit;

internal static class MessagingMetrics
{
    private static Histogram OperationDuration => Metrics.CreateHistogram("message_processing_duration", "Message processing duration", ["result", "operationDescription"]);
    private static Counter OperationCounter => Metrics.CreateCounter("message_processed_count_total", "Total count of processed messages", ["result", "operationDescription"]);
    private static Gauge CurrentProcessingMessages => Metrics.CreateGauge("messages_currently_processing", "Count of messages currently being processed", ["operationDescription"]);

    public static void ObserveSuccessfullyOperationDuration(string operationDescription, long millisecondsDuration)
    {
        OperationDuration.WithLabels(["successfully", operationDescription]).Observe(millisecondsDuration);
        OperationCounter.WithLabels(["successfully", operationDescription]).Inc();
    }

    public static void ObserveExceptionOperationDuration(string operationDescription, long millisecondsDuration)
    {
        OperationDuration.WithLabels(["exception", operationDescription]).Observe(millisecondsDuration);
        OperationCounter.WithLabels(["exception", operationDescription]).Inc();
    }

    public static void MessageProcessingStarted()
    {
        CurrentProcessingMessages.Inc();
    }

    public static void MessageProcessingFinished()
    {
        CurrentProcessingMessages.Dec();
    }
}