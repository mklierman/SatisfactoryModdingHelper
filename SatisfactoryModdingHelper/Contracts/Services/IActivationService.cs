namespace SatisfactoryModdingHelper.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
