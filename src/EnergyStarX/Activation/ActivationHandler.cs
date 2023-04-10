namespace EnergyStarX.Activation;

// Extend this class to implement new ActivationHandlers. See DefaultActivationHandler for an example.
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/activation.md
public abstract class ActivationHandler<T> : IActivationHandler
    where T : class
{
    // Override this method to add the logic for whether to handle the activation.
    protected virtual bool CanHandleInternal(T args) => true;

    // Override this method to add the logic for your activation handler.
    protected abstract Task HandleInternal(T args);

    public bool CanHandle(object args) => args is T && CanHandleInternal((args as T)!);

    public async Task Handle(object args) => await HandleInternal((args as T)!);
}
