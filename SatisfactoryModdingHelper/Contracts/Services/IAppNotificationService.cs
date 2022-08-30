using System.Collections.Specialized;

namespace SatisfactoryModdingHelper.Contracts.Services;

public interface IAppNotificationService
{
    void Initialize();

    bool Show(string payload);

    void SendNotification(string text);

    NameValueCollection ParseArguments(string arguments);

    void Unregister();
}
