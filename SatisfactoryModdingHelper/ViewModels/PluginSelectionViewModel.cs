using Microsoft.Toolkit.Mvvm.ComponentModel;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SatisfactoryModdingHelper.ViewModels
{
    public class PluginSelectionViewModel : ObservableObject, INavigationAware
    {
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private readonly string projectLocation;
        public PluginSelectionViewModel(IPersistAndRestoreService persistAndRestoreService)
        {
            _persistAndRestoreService = persistAndRestoreService;
            projectLocation = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_Project);
            PopulatePluginList();
            selectedPlugin = _persistAndRestoreService.GetSavedProperty(Properties.Resources.SelectedPlugin);
            PluginComboBoxEnabled = true;
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

        private bool pluginComboBoxEnabled;
        public bool PluginComboBoxEnabled { get => pluginComboBoxEnabled; set => SetProperty(ref pluginComboBoxEnabled, value); }

        private System.Collections.IEnumerable pluginList;
        public System.Collections.IEnumerable PluginList { get => pluginList; set => SetProperty(ref pluginList, value); }

        private void PopulatePluginList()
        {
            var pluginDirectory = projectLocation + "//Plugins";
            if (Directory.Exists(pluginDirectory))
            {
                List<string> pluginDirs = new();
                foreach (var directory in Directory.GetDirectories(pluginDirectory))
                {
                    var di = new DirectoryInfo(directory);
                    pluginDirs.Add(di.Name);
                }
                PluginList = pluginDirs;
            }
        }

        public void OnNavigatedTo(object parameter)
        {
            //throw new NotImplementedException();
        }

        public void OnNavigatedFrom()
        {
            //throw new NotImplementedException();
        }

        public void OnStartingNavigateFrom()
        {
        }
    }


}
