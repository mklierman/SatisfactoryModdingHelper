using SatisfactoryModdingHelper.Models;

namespace SatisfactoryModdingHelper.Contracts.Services;

public interface ILocalSettingsService
{
    Task<T?> ReadSettingAsync<T>(string key);

    Task SaveSettingAsync<T>(string key, T value);

    void RestoreData();

    void PersistData();

    SavedSettings Settings
    {
        get; set;
    }
}
