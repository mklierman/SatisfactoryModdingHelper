using System;
using System.Collections.Generic;
using System.Text;

namespace SatisfactoryModdingHelper.Models
{
    public class Plugin
    {
        public string Name { get; set; }
        public string SemVersion { get; set; }
        public bool Enabled { get; set; }
    }

    public class Module
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string LoadingPhase { get; set; }
    }

    public class UPluginModel
    {
        public int FileVersion { get; set; }
        public int Version { get; set; }
        public string VersionName { get; set; }
        public string SemVersion { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByURL { get; set; }
        public string DocsURL { get; set; }
        public string MarketplaceURL { get; set; }
        public string SupportURL { get; set; }
        public bool CanContainContent { get; set; }
        public bool IsBetaVersion { get; set; }
        public bool IsExperimentalVersion { get; set; }
        public bool Installed { get; set; }
        public bool AcceptsAnyRemoteVersion { get; set; }
        public List<Plugin> Plugins { get; set; }
        public List<Module> Modules { get; set; }
    }
}
