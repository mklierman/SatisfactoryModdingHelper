using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Windows.System;

namespace SatisfactoryModdingHelper.Models
{
    public class PluginModel : IEquatable<PluginModel>
    {
        public PluginModel()
        {
            Name = "";
            SemVersion = "";
            Enabled = false;
        }
        public string Name { get; set; }
        public string SemVersion { get; set; }
        public bool Enabled { get; set; }

        public bool Equals(PluginModel other)
        {
            var thisValues = this.GetType().GetProperties().Select(p => p.GetValue(this))
    .Select(o => Object.ReferenceEquals(o, null) ? default(string) : o.ToString());
            var otherValues = other.GetType().GetProperties().Select(p => p.GetValue(other))
                .Select(o => Object.ReferenceEquals(o, null) ? default(string) : o.ToString());
            return Enumerable.SequenceEqual(thisValues, otherValues);
        }
    }

    public class ModuleModel : IEquatable<ModuleModel>
    {
        public ModuleModel()
        {
            Name = "";
            Type = "";
            LoadingPhase = "";
        }
        public string Name { get; set; }
        public string Type { get; set; }
        public string LoadingPhase { get; set; }

        public bool Equals(ModuleModel other)
        {
            var thisValues = this.GetType().GetProperties().Select(p => p.GetValue(this))
    .Select(o => Object.ReferenceEquals(o, null) ? default(string) : o.ToString());
            var otherValues = other.GetType().GetProperties().Select(p => p.GetValue(other))
                .Select(o => Object.ReferenceEquals(o, null) ? default(string) : o.ToString());
            return Enumerable.SequenceEqual(thisValues, otherValues);
        }
    }

    public class UPluginModel : IEquatable<UPluginModel>
    {
        public UPluginModel()
        {
            FileVersion = 3;
            Version = 0;
            VersionName = "";
            SemVersion = "";
            FriendlyName = "";
            Description = "";
            Category = "";
            CreatedBy = "";
            CreatedByURL = "";
            DocsURL = "";
            MarketplaceURL = "";
            SupportURL = "";
            CanContainContent = false;
            IsBetaVersion = false;
            IsExperimentalVersion = false;
            Installed = false;
            AcceptsAnyRemoteVersion = false;
            Plugins = new ObservableCollection<PluginModel>();
            Modules = new ObservableCollection<ModuleModel>();
        }
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
        public ObservableCollection<PluginModel> Plugins { get; set; }
        public ObservableCollection<ModuleModel> Modules { get; set; }

        public bool Equals(UPluginModel other)
        {
            var thisValues = this.GetType().GetProperties().Select(p => p.GetValue(this))
    .Select(o => Object.ReferenceEquals(o, null) ? default(string) : o.ToString());
            var otherValues = other.GetType().GetProperties().Select(p => p.GetValue(other))
                .Select(o => Object.ReferenceEquals(o, null) ? default(string) : o.ToString());
            var eq1 = Enumerable.SequenceEqual(thisValues, otherValues);
            bool pluginEq = false;
            if (this.Plugins != null && other.Plugins != null)
            {
                pluginEq = Enumerable.SequenceEqual(this.Plugins, other.Plugins);
            }
            else if (this.Plugins == null && other.Plugins == null)
            {
                pluginEq = true;
            }
            bool moduleEq = false;
            if (this.Modules != null && other.Modules != null)
            {
                moduleEq = Enumerable.SequenceEqual(this.Modules, other.Modules);
            }
            else if (this.Modules == null && other.Modules == null)
            {
                moduleEq = true;
            }
            return eq1 && pluginEq && moduleEq;
        }
    }
}
