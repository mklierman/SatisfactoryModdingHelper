﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Models;
using System.Windows.Input;
using System.Collections.ObjectModel;
using PeanutButter.TinyEventAggregator;
using System.Linq;
using System.Windows;

namespace SatisfactoryModdingHelper.ViewModels
{
    public class UPluginViewModel : ObservableObject, INavigationAware
    {
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private readonly IPluginService _pluginService;
        private readonly IFileService _fileService;
        private readonly EventAggregator _eventAggregator;
        private SubscriptionToken pluginSelectedToken;
        private UPluginModel loadedUPlugin;
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
        private ObservableCollection<PluginModel> plugins;
        private ObservableCollection<ModuleModel> modules;


        public UPluginViewModel(IPersistAndRestoreService persistAndRestoreService, IPluginService pluginService, IFileService fileService)
        {
            _persistAndRestoreService = persistAndRestoreService;
            _pluginService = pluginService;
            _fileService = fileService;
            _eventAggregator = EventAggregator.Instance;
        }

        private UPluginModel GetSelectedPluginModel()
        {
            if (string.IsNullOrEmpty(projectDirectory) || SelectedPlugin == null)
            {
                return new UPluginModel();
            }
            string upluginText = File.ReadAllText(@$"{projectDirectory}/Plugins/{SelectedPlugin}/{SelectedPlugin}.uplugin");
            return JsonConvert.DeserializeObject<UPluginModel>(upluginText);
        }

        private void PopulateUPluginFields()
        {
            loadedUPlugin = GetSelectedPluginModel();
            FileVersion = loadedUPlugin.FileVersion.ToString();
            Version = loadedUPlugin.Version.ToString();
            VersionName = loadedUPlugin.VersionName;
            SemVersion = loadedUPlugin.SemVersion;
            FriendlyName = loadedUPlugin.FriendlyName;
            Description = loadedUPlugin.Description;
            Category = loadedUPlugin.Category;
            CreatedBy = loadedUPlugin.CreatedBy;
            createdByURL = loadedUPlugin.CreatedByURL;
            DocsURL = loadedUPlugin.DocsURL;
            MarketplaceURL = loadedUPlugin.MarketplaceURL;
            SupportURL = loadedUPlugin.SupportURL;
            CanContainContent = loadedUPlugin.CanContainContent;
            IsBetaVersion = loadedUPlugin.IsBetaVersion;
            IsExperimentalVersion = loadedUPlugin.IsExperimentalVersion;
            Installed = loadedUPlugin.Installed;
            AcceptsAnyRemoteVersion = loadedUPlugin.AcceptsAnyRemoteVersion;
            Plugins = loadedUPlugin.Plugins;
            Modules = loadedUPlugin.Modules;
        }

        public void OnNavigatedTo(object parameter)
        {
            projectDirectory = _persistAndRestoreService.Settings.ProjectPath;
            SelectedPlugin = _pluginService.SelectedPlugin;
            PopulateUPluginFields();

            pluginSelectedToken = _eventAggregator.GetEvent<PluginSelectedEvent>().Subscribe(PluginSelected);
        }

        public void OnNavigatedFrom()
        {

            _eventAggregator.GetEvent<PluginSelectedEvent>().Unsubscribe(pluginSelectedToken);
        }

        internal void PluginSelected(object plugin)
        {
            SelectedPlugin = _pluginService.SelectedPlugin;
            PopulateUPluginFields();
        }

        public string FileVersion
        {
            get => fileVersion;
            set
            {
                SetProperty(ref fileVersion, value);
                loadedUPlugin.FileVersion = int.Parse(value);
            }
        }
        public string Version
        {
            get => version;
            set
            {
                SetProperty(ref version, value);
                loadedUPlugin.Version = int.Parse(value);
            }
        }
        public string VersionName
        {
            get => versionName; set
            {
                SetProperty(ref versionName, value);
                loadedUPlugin.VersionName = value;
            }
        }
        public string SemVersion
        {
            get => semVersion; set
            {
                SetProperty(ref semVersion, value);
                loadedUPlugin.SemVersion = value;
            }
        }
        public string FriendlyName
        {
            get => friendlyName; set
            {
                SetProperty(ref friendlyName, value);
                loadedUPlugin.FriendlyName = value;
            }
        }
        public string Description
        {
            get => description; set
            {
                SetProperty(ref description, value);
                loadedUPlugin.Description = value;
            }
        }
        public string Category
        {
            get => category; set
            {
                SetProperty(ref category, value);
                loadedUPlugin.Category = value;
            }
        }
        public string CreatedBy
        {
            get => createdBy; set
            {
                SetProperty(ref createdBy, value);
                loadedUPlugin.CreatedBy = value;
            }
        }
        public string CreatedByURL
        {
            get => createdByURL; set
            {
                SetProperty(ref createdByURL, value);
                loadedUPlugin.CreatedByURL = value;
            }
        }
        public string DocsURL
        {
            get => docsURL; set
            {
                SetProperty(ref docsURL, value);
                loadedUPlugin.DocsURL = value;
            }
        }
        public string MarketplaceURL
        {
            get => marketplaceURL; set
            {
                SetProperty(ref marketplaceURL, value);
                loadedUPlugin.MarketplaceURL = value;
            }
        }
        public string SupportURL
        {
            get => supportURL; set
            {
                SetProperty(ref supportURL, value);
                loadedUPlugin.SupportURL = value;
            }
        }
        public bool CanContainContent
        {
            get => canContainContent; set
            {
                SetProperty(ref canContainContent, value);
                loadedUPlugin.CanContainContent = value;
            }
        }
        public bool IsBetaVersion
        {
            get => isBetaVersion; set
            {
                SetProperty(ref isBetaVersion, value);
                loadedUPlugin.IsBetaVersion = value;
            }
        }
        public bool IsExperimentalVersion
        {
            get => isExperimentalVersion; set
            {
                SetProperty(ref isExperimentalVersion, value);
                loadedUPlugin.IsExperimentalVersion = value;
            }
        }
        public bool Installed
        {
            get => installed; set
            {
                SetProperty(ref installed, value);
                loadedUPlugin.Installed = value;
            }
        }
        public bool AcceptsAnyRemoteVersion
        {
            get => acceptsAnyRemoteVersion; set
            {
                SetProperty(ref acceptsAnyRemoteVersion, value);
                loadedUPlugin.AcceptsAnyRemoteVersion = value;
            }
        }
        public ObservableCollection<PluginModel> Plugins
        {
            get => plugins; set
            {
                SetProperty(ref plugins, value);
                loadedUPlugin.Plugins = value;
            }
        }
        public ObservableCollection<ModuleModel> Modules
        {
            get => modules; set
            {
                SetProperty(ref modules, value);
                loadedUPlugin.Modules = value;
            }
        }

        private object selectedPlugin;


        public object SelectedPlugin {
            get => selectedPlugin;
            set
            {
                if (SetProperty(ref selectedPlugin, value))
                {
                    PopulateUPluginFields();
                }
            }
        }

        private System.Collections.IEnumerable pluginList;

        public System.Collections.IEnumerable PluginList { get => pluginList; set {
                SetProperty(ref pluginList, value);
            } }

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
                AcceptsAnyRemoteVersion = AcceptsAnyRemoteVersion,
                Plugins = Plugins,
                Modules = Modules
            };

            _fileService.Save(folderPath, fileName, uPlugin);
        }

        private RelayCommand cancelUPluginChanges;
        public ICommand CancelUPluginChanges => cancelUPluginChanges ??= new RelayCommand(PerformCancelUPluginChanges);

        private void PerformCancelUPluginChanges()
        {
            PopulateUPluginFields();
        }

        private RelayCommand addPlugin;
        public ICommand AddPlugin => addPlugin ??= new RelayCommand(PerformAddPlugin);

        private void PerformAddPlugin()
        {
            if (Plugins == null)
            {
                Plugins = new ObservableCollection<PluginModel>();
            }
            Plugins.Add(new PluginModel());
        }

        private RelayCommand<string> removePlugin;
        public ICommand RemovePlugin => removePlugin ??= new RelayCommand<string>(param => this.PerformRemovePlugin(param));

        private void PerformRemovePlugin(string pluginName)
        {
            Plugins.Remove(Plugins.FirstOrDefault(p => p.Name == pluginName));
        }

        private bool PluginModified()
        {
            if (!GetSelectedPluginModel().Equals(loadedUPlugin))
            {
                return true;
            }
            return false;
        }

        private MessageBoxResult DisplaySavePopup()
        {
            string messageBoxText = "You have unsaved changes. Do you want to save before navigating away?";
            string caption = "Unsaved Changes";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
            return result;
        }

        public void OnStartingNavigateFrom()
        {
            if (PluginModified())
            {
                MessageBoxResult result = DisplaySavePopup();
                if (result == MessageBoxResult.Yes)
                {
                    PerformSaveUPlugin();
                }
            }
        }

        private RelayCommand addModule;
        public ICommand AddModule => addModule ??= new RelayCommand(PerformAddModule);

        private void PerformAddModule()
        {
            if (Modules == null)
            {
                Modules = new ObservableCollection<ModuleModel>();
            }
            Modules.Add(new ModuleModel());
        }

        private RelayCommand<string> removeModule;
        public ICommand RemoveModule => removeModule ??= new RelayCommand<string>(param => this.PerformRemoveModule(param));

        private void PerformRemoveModule(string moduleName)
        {
            Modules.Remove(Modules.FirstOrDefault(m => m.Name == moduleName));
        }
    }
}
