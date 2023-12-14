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

public class ModService : ObservableRecipient, IModService
{
    private readonly ILocalSettingsService _settingsService;
    private string projectLocation;

    public ModService(ILocalSettingsService settingsService)
    {
        selectedMod = new object();
        _settingsService = settingsService;
        projectLocation = _settingsService.Settings.UProjectFolderPath;
        SelectedMod = _settingsService.Settings.CurrentMod;
    }

    private object selectedMod;

    public event EventHandler<object> ModChangedEvent;

    public object SelectedMod
    {
        get => selectedMod;
        set
        {
            SetProperty(ref selectedMod, value);
            if (value != null)
            {

                _settingsService.Settings.CurrentMod = value.ToString() ?? "";
                _settingsService.PersistData();
                ModChangedEvent?.Invoke(this, value);
            }
        }
    }

    private readonly string[] ignoredItems = {"wwise", "alpakit", "smleditor", "abstractinstance", "sml", "examplemod" };

    public IEnumerable? ModList => GetModList();

    public IEnumerable? GetModList()
    {
        projectLocation = _settingsService.Settings.UProjectFolderPath;
        var modDirectory = projectLocation + "\\Mods";
        if (Directory.Exists(modDirectory))
        {
            List<string> modDirs = new();
            foreach (var directory in Directory.GetDirectories(modDirectory))
            {
                var di = new DirectoryInfo(directory);
                if (!ignoredItems.Contains(di.Name.ToLower()))
                {
                    modDirs.Add(di.Name);
                }
            }
            return modDirs;
        }
        return null;
    }
}
