using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Newtonsoft.Json;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Core.Contracts.Services;
using SatisfactoryModdingHelper.Models;

namespace SatisfactoryModdingHelper.ViewModels;

public class UPluginViewModel : ObservableObject, INavigationAware
{
    private readonly IPluginService _pluginService;
    private readonly IFileService _fileService;
    private readonly ILocalSettingsService _settingsService;
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
    public UPluginViewModel(IPluginService pluginService, IFileService fileService, ILocalSettingsService settingsService)
    {
        _pluginService = pluginService;
        _fileService = fileService;
        _settingsService = settingsService;
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
        CreatedByURL = loadedUPlugin.CreatedByURL;
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
        if (Plugins?.Count > 0)
        {
            SelectedDepPlugin = Plugins[^1];
        }
    }

    public void OnNavigatedTo(object parameter)
    {
        projectDirectory = _settingsService.Settings.ProjectPath;
        SelectedPlugin = _pluginService.SelectedPlugin;
        PopulateUPluginFields();
        _pluginService.PluginChangedEvent += OnPluginChanged;

    }

    public void OnNavigatedFrom()
    {

        _pluginService.PluginChangedEvent -= OnPluginChanged;
    }

    private void OnPluginChanged(object? sender, object e)
    {
        SelectedPlugin = e;
        PopulateUPluginFields();
    }

    public string FileVersion
    {
        get => fileVersion;
        set
        {
            SetProperty(ref fileVersion, value);
            loadedUPlugin.FileVersion = int.Parse(value);
            CheckIfNeedsSave();
        }
    }
    public string Version
    {
        get => version;
        set
        {
            SetProperty(ref version, value);
            loadedUPlugin.Version = int.Parse(value);
            CheckIfNeedsSave();
        }
    }
    public string VersionName
    {
        get => versionName; set
        {
            SetProperty(ref versionName, value);
            loadedUPlugin.VersionName = value;
            CheckIfNeedsSave();
        }
    }
    public string SemVersion
    {
        get => semVersion; set
        {
            SetProperty(ref semVersion, value);
            loadedUPlugin.SemVersion = value;
            CheckIfNeedsSave();
        }
    }
    public string FriendlyName
    {
        get => friendlyName; set
        {
            SetProperty(ref friendlyName, value);
            loadedUPlugin.FriendlyName = value;
            CheckIfNeedsSave();
        }
    }
    public string Description
    {
        get => description; set
        {
            SetProperty(ref description, value);
            loadedUPlugin.Description = value;
            CheckIfNeedsSave();
        }
    }
    public string Category
    {
        get => category; set
        {
            SetProperty(ref category, value);
            loadedUPlugin.Category = value;
            CheckIfNeedsSave();
        }
    }
    public string CreatedBy
    {
        get => createdBy; set
        {
            SetProperty(ref createdBy, value);
            loadedUPlugin.CreatedBy = value;
            CheckIfNeedsSave();
        }
    }
    public string CreatedByURL
    {
        get => createdByURL; set
        {
            SetProperty(ref createdByURL, value);
            loadedUPlugin.CreatedByURL = value;
            CheckIfNeedsSave();
        }
    }
    public string DocsURL
    {
        get => docsURL; set
        {
            SetProperty(ref docsURL, value);
            loadedUPlugin.DocsURL = value;
            CheckIfNeedsSave();
        }
    }
    public string MarketplaceURL
    {
        get => marketplaceURL; set
        {
            SetProperty(ref marketplaceURL, value);
            loadedUPlugin.MarketplaceURL = value;
            CheckIfNeedsSave();
        }
    }
    public string SupportURL
    {
        get => supportURL; set
        {
            SetProperty(ref supportURL, value);
            loadedUPlugin.SupportURL = value;
            CheckIfNeedsSave();
        }
    }
    public bool CanContainContent
    {
        get => canContainContent; set
        {
            SetProperty(ref canContainContent, value);
            loadedUPlugin.CanContainContent = value;
            CheckIfNeedsSave();
        }
    }
    public bool IsBetaVersion
    {
        get => isBetaVersion; set
        {
            SetProperty(ref isBetaVersion, value);
            loadedUPlugin.IsBetaVersion = value;
            CheckIfNeedsSave();
        }
    }
    public bool IsExperimentalVersion
    {
        get => isExperimentalVersion; set
        {
            SetProperty(ref isExperimentalVersion, value);
            loadedUPlugin.IsExperimentalVersion = value;
            CheckIfNeedsSave();
        }
    }
    public bool Installed
    {
        get => installed; set
        {
            SetProperty(ref installed, value);
            loadedUPlugin.Installed = value;
            CheckIfNeedsSave();
        }
    }
    public bool AcceptsAnyRemoteVersion
    {
        get => acceptsAnyRemoteVersion; set
        {
            SetProperty(ref acceptsAnyRemoteVersion, value);
            loadedUPlugin.AcceptsAnyRemoteVersion = value;
            CheckIfNeedsSave();
        }
    }
    public ObservableCollection<PluginModel> Plugins
    {
        get => plugins; set
        {
            SetProperty(ref plugins, value);
            loadedUPlugin.Plugins = value;
            CheckIfNeedsSave();
        }
    }
    public ObservableCollection<ModuleModel> Modules
    {
        get => modules; set
        {
            SetProperty(ref modules, value);
            loadedUPlugin.Modules = value;
            CheckIfNeedsSave();
        }
    }

    private object selectedPlugin;


    public object SelectedPlugin
    {
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

    public System.Collections.IEnumerable PluginList
    {
        get => pluginList; set
        {
            SetProperty(ref pluginList, value);
        }
    }

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
        CheckIfNeedsSave();
    }

    private RelayCommand cancelUPluginChanges;
    public ICommand CancelUPluginChanges => cancelUPluginChanges ??= new RelayCommand(PerformCancelUPluginChanges);

    private void PerformCancelUPluginChanges()
    {
        PopulateUPluginFields();
        CheckIfNeedsSave();
    }

    private RelayCommand addPlugin;
    public ICommand AddPlugin => addPlugin ??= new RelayCommand(PerformAddPlugin);

    private void PerformAddPlugin()
    {
        if (Plugins == null)
        {
            Plugins = new ObservableCollection<PluginModel>();
        }
        Plugins.Add(new PluginModel("New Plugin Dependency", "1.0.0", true));
        SelectedDepPlugin = Plugins[^1];
        CheckIfNeedsSave();
    }

    private RelayCommand removePlugin;
    public ICommand RemovePlugin => removePlugin ??= new RelayCommand(PerformRemovePlugin);

    private void PerformRemovePlugin()
    {
        var oldIndex = Plugins.IndexOf(SelectedDepPlugin);
        Plugins.Remove(SelectedDepPlugin);
        if (oldIndex > 0)
        {
            SelectedDepPlugin = Plugins[oldIndex-1];
        }
        CheckIfNeedsSave();
    }

    private void CheckIfNeedsSave()
    {
        if (PluginModified())
        {
            UnsavedChanges = true;
        }
        else
        {
            UnsavedChanges = false;
        }
    }

    private bool PluginModified()
    {
        if (!GetSelectedPluginModel().Equals(loadedUPlugin))
        {
            return true;
        }
        return false;
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
        CheckIfNeedsSave();
    }

    private RelayCommand<string> removeModule;
    public ICommand RemoveModule => removeModule ??= new RelayCommand<string>(param => this.PerformRemoveModule(param));

    private void PerformRemoveModule(string moduleName)
    {
        Modules.Remove(Modules.FirstOrDefault(m => m.Name == moduleName));
        CheckIfNeedsSave();
    }

    private bool unsavedChanges = false;
    public bool UnsavedChanges
    {
        get => unsavedChanges;
        set
        {
            SetProperty(ref unsavedChanges, value);
            SaveCancelVisibility = value ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private Visibility saveCancelVisibility = Visibility.Collapsed;
    public Visibility SaveCancelVisibility
    {
        get => saveCancelVisibility;
        set => SetProperty(ref saveCancelVisibility, value);
    }


    private PluginModel selectedDepPlugin;
    public PluginModel SelectedDepPlugin
    {
        get => selectedDepPlugin;
        set => SetProperty(ref selectedDepPlugin, value);
    }
}
