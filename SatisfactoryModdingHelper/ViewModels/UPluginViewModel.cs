using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Models;
using System.Windows.Input;

namespace SatisfactoryModdingHelper.ViewModels
{
    public class UPluginViewModel : ObservableObject, INavigationAware
    {
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private readonly IPluginService _pluginService;
        private readonly IFileService _fileService;
        private string projectDirectory;
        private string fileVersion;
        private string version;
        private string versionName;
        private string semVersion;
        private string friendlyName;
        private string description;
        private string category;
        private string createdBy;
        private string createdByURL;
        private string docsURL;
        private string marketplaceURL;
        private string supportURL;
        private bool canContainContent;
        private bool isBetaVersion;
        private bool isExperimentalVersion;
        private bool installed;
        private bool acceptsAnyRemoteVersion;
        private List<string> plugins;
        private List<string> modules;


        public UPluginViewModel(IPersistAndRestoreService persistAndRestoreService, IPluginService pluginService, IFileService fileService)
        {
            _persistAndRestoreService = persistAndRestoreService;
            _pluginService = pluginService;
            _fileService = fileService;
            //selectedPlugin = _persistAndRestoreService.GetSavedProperty(Properties.Resources.SelectedPlugin);
            //PluginSelectorViewModel = new PluginSelectionViewModel(persistAndRestoreService);
        }

        private void PopulateUPluginFields()
        {
            if (string.IsNullOrEmpty(projectDirectory) || SelectedPlugin == null)
            {
                return;
            }
            string upluginText = File.ReadAllText(@$"{projectDirectory}/Plugins/{SelectedPlugin}/{SelectedPlugin}.uplugin");
            UPluginModel uPlugin = JsonConvert.DeserializeObject<UPluginModel>(upluginText);
            FileVersion = uPlugin.FileVersion.ToString();
            Version = uPlugin.Version.ToString();
            VersionName = uPlugin.VersionName;
            SemVersion = uPlugin.SemVersion;
            FriendlyName = uPlugin.FriendlyName;
            Description = uPlugin.Description;
            Category = uPlugin.Category;
            CreatedBy = uPlugin.CreatedBy;
            createdByURL = uPlugin.CreatedByURL;
            DocsURL = uPlugin.DocsURL;
            MarketplaceURL = uPlugin.MarketplaceURL;
            SupportURL = uPlugin.SupportURL;
            CanContainContent = uPlugin.CanContainContent;
            IsBetaVersion = uPlugin.IsBetaVersion;
            IsExperimentalVersion = uPlugin.IsExperimentalVersion;
            Installed = uPlugin.Installed;
            AcceptsAnyRemoteVersion = uPlugin.AcceptsAnyRemoteVersion;
        }

        public void OnNavigatedTo(object parameter)
        {
            projectDirectory = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_Project);
            SelectedPlugin = _pluginService.SelectedPlugin;
            PluginList = _pluginService.PluginList;
            PopulateUPluginFields();
        }

        public void OnNavigatedFrom()
        {
            _pluginService.SelectedPlugin = SelectedPlugin;
            _persistAndRestoreService.PersistData();
        }

        public string FileVersion { get => fileVersion; set => SetProperty(ref fileVersion, value); }
        public string Version { get => version; set => SetProperty(ref version, value); }
        public string VersionName { get => versionName; set => SetProperty(ref versionName, value); }
        public string SemVersion { get => semVersion; set => SetProperty(ref semVersion, value); }
        public string FriendlyName { get => friendlyName; set => SetProperty(ref friendlyName, value); }
        public string Description { get => description; set => SetProperty(ref description, value); }
        public string Category { get => category; set => SetProperty(ref category, value); }
        public string CreatedBy { get => createdBy; set => SetProperty(ref createdBy, value); }
        public string CreatedByURL { get => createdByURL; set => SetProperty(ref createdByURL, value); }
        public string DocsURL { get => docsURL; set => SetProperty(ref docsURL, value); }
        public string MarketplaceURL { get => marketplaceURL; set => SetProperty(ref marketplaceURL, value); }
        public string SupportURL { get => supportURL; set => SetProperty(ref supportURL, value); }
        public bool CanContainContent { get => canContainContent; set => SetProperty(ref canContainContent, value); }
        public bool IsBetaVersion { get => isBetaVersion; set => SetProperty(ref isBetaVersion, value); }
        public bool IsExperimentalVersion { get => isExperimentalVersion; set => SetProperty(ref isExperimentalVersion, value); }
        public bool Installed { get => installed; set => SetProperty(ref installed, value); }
        public bool AcceptsAnyRemoteVersion { get => acceptsAnyRemoteVersion; set => SetProperty(ref acceptsAnyRemoteVersion, value); }
        public List<string> Plugins { get => plugins; set => SetProperty(ref plugins, value); }
        public List<string> Modules { get => modules; set => SetProperty(ref modules, value); }

        private object pluginSelectorViewModel;

        public object PluginSelectorViewModel { get => pluginSelectorViewModel; set => SetProperty(ref pluginSelectorViewModel, value); }

        private object selectedPlugin;

        public object SelectedPlugin {
            get => selectedPlugin;
            set
            {
                if (SetProperty(ref selectedPlugin, value))
                {
                    PopulateUPluginFields();
                    _persistAndRestoreService.SaveProperty(Properties.Resources.SelectedPlugin, value);
                }
            }
        }

        private System.Collections.IEnumerable pluginList;

        public System.Collections.IEnumerable PluginList { get => pluginList; set => SetProperty(ref pluginList, value); }

        private RelayCommand saveUPlugin;
        public ICommand SaveUPlugin => saveUPlugin ??= new RelayCommand(PerformSaveUPlugin);

        private void PerformSaveUPlugin()
        {
            if (string.IsNullOrEmpty(projectDirectory) || SelectedPlugin == null)
            {
                return;
            }
            string folderPath = @$"{projectDirectory}/Plugins/{SelectedPlugin}/";
            string fileName = $@"{SelectedPlugin}.uplugin";
            UPluginModel uPlugin = new()
            {
                FileVersion = int.Parse(FileVersion),
                Version = int.Parse(Version),
                VersionName = VersionName,
                SemVersion = SemVersion,
                FriendlyName = FriendlyName,
                Description = Description,
                Category = Category,
                CreatedBy = CreatedBy,
                CreatedByURL = CreatedByURL,
                DocsURL = DocsURL,
                MarketplaceURL = MarketplaceURL,
                SupportURL = SupportURL,
                CanContainContent = CanContainContent,
                IsBetaVersion = IsBetaVersion,
                IsExperimentalVersion = IsExperimentalVersion,
                Installed = Installed,
                AcceptsAnyRemoteVersion = AcceptsAnyRemoteVersion
            };
            //var serializedJson = JsonConvert.SerializeObject(uPlugin);
            _fileService.Save(folderPath, fileName, uPlugin);
            FileVersion = uPlugin.FileVersion.ToString();
            Version = uPlugin.Version.ToString();
            VersionName = uPlugin.VersionName;
            SemVersion = uPlugin.SemVersion;
            FriendlyName = uPlugin.FriendlyName;
            Description = uPlugin.Description;
            Category = uPlugin.Category;
            CreatedBy = uPlugin.CreatedBy;
            CreatedByURL = uPlugin.CreatedByURL;
            DocsURL = uPlugin.DocsURL;
            MarketplaceURL = uPlugin.MarketplaceURL;
            SupportURL = uPlugin.SupportURL;
            CanContainContent = uPlugin.CanContainContent;
            IsBetaVersion = uPlugin.IsBetaVersion;
            IsExperimentalVersion = uPlugin.IsExperimentalVersion;
            Installed = uPlugin.Installed;
            AcceptsAnyRemoteVersion = uPlugin.AcceptsAnyRemoteVersion;
        }

        private RelayCommand cancelUPluginChanges;
        public ICommand CancelUPluginChanges => cancelUPluginChanges ??= new RelayCommand(PerformCancelUPluginChanges);

        private void PerformCancelUPluginChanges()
        {
            PopulateUPluginFields();
        }
    }
}
