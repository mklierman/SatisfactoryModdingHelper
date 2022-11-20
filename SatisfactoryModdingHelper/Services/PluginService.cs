using CommunityToolkit.Mvvm.ComponentModel;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryModdingHelper.Services;

public class PluginService : ObservableRecipient, IPluginService
{
    private readonly ILocalSettingsService _settingsService;
    private string projectLocation;

    public PluginService(ILocalSettingsService settingsService)
    {
        selectedPlugin = new object();
        _settingsService = settingsService;
        projectLocation = _settingsService.Settings.UProjectFolderPath;
        SelectedPlugin = _settingsService.Settings.CurrentPlugin;
    }

    private object selectedPlugin;

    public event EventHandler<object> PluginChangedEvent;

    public object SelectedPlugin
    {
        get => selectedPlugin;
        set
        {
            SetProperty(ref selectedPlugin, value);
            if (value != null)
            {

                _settingsService.Settings.CurrentPlugin = value.ToString() ?? "";
                _settingsService.PersistData();
                PluginChangedEvent?.Invoke(this, value);
            }
        }
    }

    public IEnumerable? PluginList => GetPluginList();

    public IEnumerable? GetPluginList()
    {
        projectLocation = _settingsService.Settings.UProjectFolderPath;
        var pluginDirectory = projectLocation + "//Plugins";
        if (Directory.Exists(pluginDirectory))
        {
            List<string> pluginDirs = new();
            foreach (var directory in Directory.GetDirectories(pluginDirectory))
            {
                var di = new DirectoryInfo(directory);
                pluginDirs.Add(di.Name);
            }
            return pluginDirs;
        }
        return null;
    }
}
