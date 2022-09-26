namespace EnergyStarX.Activation;

public interface IActivationHandler
{
    bool CanHandle(object args);

    Task Handle(object args);
}
