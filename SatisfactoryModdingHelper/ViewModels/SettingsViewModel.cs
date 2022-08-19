using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Helpers;
using SatisfactoryModdingHelper.Services;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SatisfactoryModdingHelper.ViewModels;

public class SettingsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalSettingsService _localSettingsService;
    private ElementTheme _elementTheme;
    private string _versionDescription;
    private FolderPicker folderPicker = new();

    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, ILocalSettingsService localSettingsService)
    {
        _themeSelectorService = themeSelectorService;
        _localSettingsService=localSettingsService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });

        folderPicker.FileTypeFilter.Add("*");
        folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    public void OnNavigatedTo(object parameter)
    {
        _localSettingsService.RestoreData();
        UnrealEngineLocation = _localSettingsService.Settings.UnrealEnginePath;
        VisualStudioLocation = _localSettingsService.Settings.VisualStudioPath;
        ProjectFolder = _localSettingsService.Settings.ProjectPath;
        SatisfactoryFolder = _localSettingsService.Settings.SatisfactoryPath;
        SMMFolder = _localSettingsService.Settings.ModManagerPath;
        AlpakitCopyPlugin = _localSettingsService.Settings.AlpakitCopyModToGame;
        AlpakitCloseSatisfactory = _localSettingsService.Settings.AlpakitCloseGame;
        MPPlayer1Name = _localSettingsService.Settings.Player1Name;
        MPPlayer2Name = _localSettingsService.Settings.Player2Name;
        MPPlayer1Args = _localSettingsService.Settings.Player1Args;
        MPPlayer2Args = _localSettingsService.Settings.Player2Args;
        MPGameLocation = _localSettingsService.Settings.Player2SatisfactoryPath;
    }
    public void OnNavigatedFrom()  => _localSettingsService.PersistData();

    private string unrealEngineLocation;

    public string UnrealEngineLocation
    {
        get => unrealEngineLocation;

        set
        {
            SetProperty(ref unrealEngineLocation, value);
            _localSettingsService.Settings.UnrealEnginePath = value;
            _localSettingsService.PersistData();
        }
    }

    private string visualStudioLocation;

    public string VisualStudioLocation
    {
        get => visualStudioLocation;
        set
        {
            SetProperty(ref visualStudioLocation, value);
            _localSettingsService.Settings.VisualStudioPath = value;
            _localSettingsService.PersistData();
        }
    }

    private string projectFolder;

    public string ProjectFolder
    {
        get => projectFolder;
        set
        {
            SetProperty(ref projectFolder, value);
            _localSettingsService.Settings.ProjectPath = value;
            _localSettingsService.PersistData();
        }
    }

    private string satisfactoryFolder;

    public string SatisfactoryFolder
    {
        get => satisfactoryFolder; set
        {
            SetProperty(ref satisfactoryFolder, value);
            _localSettingsService.Settings.SatisfactoryPath = value;
            _localSettingsService.PersistData();
        }
    }

    private string sMMFolder;

    public string SMMFolder
    {
        get => sMMFolder; set
        {
            SetProperty(ref sMMFolder, value);
            _localSettingsService.Settings.ModManagerPath = value;
            _localSettingsService.PersistData();
        }
    }

    private AsyncRelayCommand browseForUELocation;
    public ICommand BrowseForUELocation => browseForUELocation ??= new AsyncRelayCommand(PerformBrowseForUELocation);

    private async Task PerformBrowseForUELocation()
    {
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, ProcessService.GetAppHWND());
        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            UnrealEngineLocation = folder.Path;
        }
    }

    private AsyncRelayCommand browseForVSLocation;
    public ICommand BrowseForVSLocation => browseForVSLocation ??= new AsyncRelayCommand(PerformBrowseForVSLocation);

    private async Task PerformBrowseForVSLocation()
    {
        //var folderBrowser = new WPFFolderBrowser.WPFFolderBrowserDialog();
        //var vs2019 = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE";
        //var vs2017 = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE";
        //var vs2015 = @"C:\Program Files (x86)\Microsoft Visual Studio\2015\Community\Common7\IDE";
        //if (Directory.Exists(vs2019))
        //{
        //    folderBrowser.InitialDirectory = vs2019;
        //}
        //else if (Directory.Exists(vs2017))
        //{
        //    folderBrowser.InitialDirectory = vs2017;
        //}
        //else
        //{
        //    folderBrowser.InitialDirectory = vs2015;
        //}
        //if (folderBrowser.ShowDialog() == true)
        //{
        //    VisualStudioLocation = folderBrowser.FileName;
        //}


        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, ProcessService.GetAppHWND());
        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            VisualStudioLocation = folder.Path;
        }
    }

    private AsyncRelayCommand browseForProjectLocation;
    public ICommand BrowseForProjectLocation => browseForProjectLocation ??= new AsyncRelayCommand(PerformBrowseForProjectLocation);

    private async Task PerformBrowseForProjectLocation()
    {
        //var folderBrowser = new WPFFolderBrowser.WPFFolderBrowserDialog();
        //folderBrowser.InitialDirectory = "C:\\";
        //if (folderBrowser.ShowDialog() == true)
        //{
        //    ProjectFolder = folderBrowser.FileName;
        //}

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, ProcessService.GetAppHWND());
        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            ProjectFolder = folder.Path;
        }
    }

    private AsyncRelayCommand browseForGameLocation;
    public ICommand BrowseForGameLocation => browseForGameLocation ??= new AsyncRelayCommand(PerformBrowseForGameLocation);

    private async Task PerformBrowseForGameLocation()
    {
        //var folderBrowser = new WPFFolderBrowser.WPFFolderBrowserDialog();
        //if (folderBrowser.ShowDialog() == true)
        //{
        //    SatisfactoryFolder = folderBrowser.FileName;
        //}

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, ProcessService.GetAppHWND());
        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            SatisfactoryFolder = folder.Path;
        }
    }

    private AsyncRelayCommand browseForSMMLocation;
    public ICommand BrowseForSMMLocation => browseForSMMLocation ??= new AsyncRelayCommand(PerformBrowseForSMMLocation);

    private async Task PerformBrowseForSMMLocation()
    {
        //var folderBrowser = new WPFFolderBrowser.WPFFolderBrowserDialog();
        //folderBrowser.InitialDirectory = $"{Environment.GetEnvironmentVariable("LocalAppData")}\\Programs\\Satisfactory Mod Manager";
        //if (folderBrowser.ShowDialog() == true)
        //{
        //    SMMFolder = folderBrowser.FileName;
        //}

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, ProcessService.GetAppHWND());
        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            SMMFolder = folder.Path;
        }
    }

    private bool alpakitCopyPlugin;

    public bool AlpakitCopyPlugin
    {
        get => alpakitCopyPlugin;
        set
        {
            SetProperty(ref alpakitCopyPlugin, value);
            _localSettingsService.Settings.AlpakitCopyModToGame = value;
            _localSettingsService.PersistData();
        }
    }

    private bool alpakitCloseSatisfactory;

    public bool AlpakitCloseSatisfactory
    {
        get => alpakitCloseSatisfactory;
        set
        {
            SetProperty(ref alpakitCloseSatisfactory, value);
            _localSettingsService.Settings.AlpakitCloseGame = value;
            _localSettingsService.PersistData();
        }
    }

    private string mPPlayer1Name;

    public string MPPlayer1Name
    {
        get => mPPlayer1Name;
        set
        {
            SetProperty(ref mPPlayer1Name, value);
            _localSettingsService.Settings.Player1Name = value;
            _localSettingsService.PersistData();
        }
    }

    private string mPPlayer2Name;

    public string MPPlayer2Name
    {
        get => mPPlayer2Name;
        set
        {
            SetProperty(ref mPPlayer2Name, value);
            _localSettingsService.Settings.Player2Name = value;
            _localSettingsService.PersistData();
        }
    }

    private string mPArgs1;

    public string MPPlayer1Args
    {
        get => mPArgs1;
        set
        {
            SetProperty(ref mPArgs1, value);
            _localSettingsService.Settings.Player1Args = value;
            _localSettingsService.PersistData();
        }
    }

    private string mPArgs2;

    public string MPPlayer2Args
    {
        get => mPArgs2;
        set
        {
            SetProperty(ref mPArgs2, value);
            _localSettingsService.Settings.Player2Args = value;
            _localSettingsService.PersistData();
        }
    }

    private string mPGameLocation;
    public string MPGameLocation
    {
        get => mPGameLocation;
        set
        {
            SetProperty(ref mPGameLocation, value);
            _localSettingsService.Settings.Player2SatisfactoryPath = value;
            _localSettingsService.PersistData();
        }
    }

    private RelayCommand browseForSecondaryGameLocation;
    public ICommand BrowseForSecondaryGameLocation => browseForSecondaryGameLocation ??= new RelayCommand(PerformBrowseForSecondaryGameLocation);

    private void PerformBrowseForSecondaryGameLocation()
    {
        var folderBrowser = new WPFFolderBrowser.WPFFolderBrowserDialog();
        if (folderBrowser.ShowDialog() == true)
        {
            MPGameLocation = folderBrowser.FileName;
        }
    }
}
