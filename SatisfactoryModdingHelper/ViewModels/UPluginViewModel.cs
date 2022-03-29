using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Models;

namespace SatisfactoryModdingHelper.ViewModels
{
    public class UPluginViewModel : ObservableObject
    {
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private readonly string projectDirectory;
        private readonly string selectedPlugin;
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


        public UPluginViewModel(IPersistAndRestoreService persistAndRestoreService)
        {
            _persistAndRestoreService = persistAndRestoreService;
            projectDirectory = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_Project);
            selectedPlugin = _persistAndRestoreService.GetSavedProperty(Properties.Resources.SelectedPlugin);
            string upluginText = File.ReadAllText(@$"{projectDirectory}/Plugins/{selectedPlugin}/{selectedPlugin}.uplugin");
            UPluginModel uPlugin = JsonConvert.DeserializeObject<UPluginModel>(upluginText);
            PopulateUPluginFields(uPlugin);
        }

        private void PopulateUPluginFields(UPluginModel model)
        {
            FileVersion = model.FileVersion.ToString();
            Version = model.Version.ToString();
            VersionName = model.VersionName;
            SemVersion = model.SemVersion;
            FriendlyName = model.FriendlyName;
            Description = model.Description;
            Category = model.Category;
            CreatedBy = model.CreatedBy;
            createdByURL = model.CreatedByURL;
            DocsURL = model.DocsURL;
            MarketplaceURL = model.MarketplaceURL;
            SupportURL = model.SupportURL;
            CanContainContent = model.CanContainContent;
            IsBetaVersion = model.IsBetaVersion;
            IsExperimentalVersion = model.IsExperimentalVersion;
            Installed = model.Installed;
            AcceptsAnyRemoteVersion = model.AcceptsAnyRemoteVersion;
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
    }
}
