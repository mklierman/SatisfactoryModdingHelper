using System;
using System.IO;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Models;

namespace SatisfactoryModdingHelper.ViewModels
{
    public class UPluginViewModel : ObservableObject
    {
        private readonly IPersistAndRestoreService _persistAndRestoreService;
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
        private bool acceptsAnyVersion;


        public UPluginViewModel(IPersistAndRestoreService persistAndRestoreService)
        {
            _persistAndRestoreService = persistAndRestoreService;

            //string upluginText = File.ReadAllText($"{pluginDirectoryLocation}//{SelectedPlugin}.uplugin");
            //UPluginModel uPlugin = JsonConvert.DeserializeObject<UPluginModel>(upluginText);


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
        public bool AcceptsAnyVersion { get => acceptsAnyVersion; set => SetProperty(ref acceptsAnyVersion, value); }
    }
}
