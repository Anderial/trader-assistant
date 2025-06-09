namespace DistributedKit.StreamHandler;

public interface IHandlerRegistrator
{
    IHandlerRegistrator Add<TImplementedHandler, TMessage>() where TImplementedHandler : class, IPersistentStreamHandlerWithGuidKey<TMessage>;
}

internal sealed class HandlerRegistrator : IHandlerRegistrator
{
    internal List<HandlerRegistratorModel> Items { get; set; } = [];

    public IHandlerRegistrator Add<TImplementedHandler, TMessage>()
        where TImplementedHandler : class, IPersistentStreamHandlerWithGuidKey<TMessage>
    {
        Items.Add(new(
            HandlerType: typeof(TImplementedHandler),
            IHandlerType: typeof(IPersistentStreamHandlerWithGuidKey<TMessage>),
            HandlerRunnerType: typeof(PersistentStreamHandlerRunner<TMessage>),
            IHandlerRunnerType: typeof(IPersistentStreamHandlerRunner)
        ));
        return this;
    }

    internal record HandlerRegistratorModel(
        Type HandlerType, Type IHandlerType,
        Type HandlerRunnerType, Type IHandlerRunnerType
    );
}