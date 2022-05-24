using Microsoft.Toolkit.Mvvm.ComponentModel;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryModdingHelper.Services
{
    public class PluginService :  ObservableObject, IPluginService
    {
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private string projectLocation;

        public PluginService(IPersistAndRestoreService persistAndRestoreService)
        {
            _persistAndRestoreService = persistAndRestoreService;
            projectLocation = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_Project);
            //PopulatePluginList();
            SelectedPlugin = _persistAndRestoreService.GetSavedProperty(Properties.Resources.SelectedPlugin);
        }

        private object selectedPlugin;
        public object SelectedPlugin
        {
            get => selectedPlugin;
            set
            {
                SetProperty(ref selectedPlugin, value);
                _persistAndRestoreService.SaveProperty(Properties.Resources.SelectedPlugin, value);
            }
        }

        private System.Collections.IEnumerable pluginList;
        public IEnumerable PluginList { get => GetPluginList(); }

        public IEnumerable GetPluginList()
        {
            projectLocation = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_Project);
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
}
