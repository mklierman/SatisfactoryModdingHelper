using System.Threading.Tasks;

namespace SatisfactoryModdingHelper.Contracts.Activation
{
    public interface IActivationHandler
    {
        bool CanHandle();

        Task HandleAsync();
    }
}
