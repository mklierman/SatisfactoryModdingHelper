using Microsoft.Windows.ApplicationModel.Resources;
using Windows.Storage;

namespace SatisfactoryModdingHelper.Helpers;

public static class ResourceHelpers
{
    private static readonly ResourceLoader _resourceLoader = new();

    public static string GetLocalized(this string resourceKey) => _resourceLoader.GetString(resourceKey);

    public static async Task<string> GetTemplateResource(string resourceName)
    {
        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Templates/{resourceName}"));
        using var stream = await file.OpenStreamForReadAsync();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static async Task<string> GetTemplateResourceAndReplace(string resourceName, string oldValue, string newValue)
    {
        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Templates/{resourceName}"));
        string fileText;
        using (var stream = await file.OpenStreamForReadAsync())
        {
            using var reader = new StreamReader(stream);
            fileText = reader.ReadToEnd();
        }
        return fileText.Replace(oldValue, newValue);
    }
}
