using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Extensions;
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
    private FileOpenPicker filePicker = new FileOpenPicker();

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
        UnrealEngineFolderPath = _localSettingsService.Settings.UnrealEngineFolderPath;
        UnrealBuildToolFilePath = _localSettingsService.Settings.UnrealBuildToolFilePath;
        UProjectFolderPath = _localSettingsService.Settings.UProjectFolderPath;
        UProjectFilePath = _localSettingsService.Settings.UProjectFilePath;
        SatisfactoryFolderPath = _localSettingsService.Settings.SatisfactoryFolderPath;
        SatisfactoryExecutableFilePath = _localSettingsService.Settings.SatisfactoryExecutableFilePath;
        ModManagerFilePath = _localSettingsService.Settings.ModManagerFilePath;
        ModManagerFolderPath = _localSettingsService.Settings.ModManagerFolderPath;
        AlpakitCopyPlugin = _localSettingsService.Settings.AlpakitCopyModToGame;
        AlpakitCloseSatisfactory = _localSettingsService.Settings.AlpakitCloseGame;
        MPPlayer1Name = _localSettingsService.Settings.Player1Name;
        MPPlayer2Name = _localSettingsService.Settings.Player2Name;
        MPPlayer1Args = _localSettingsService.Settings.Player1Args;
        MPPlayer2Args = _localSettingsService.Settings.Player2Args;
        MPGameLocation = _localSettingsService.Settings.Player2SatisfactoryPath;

        if (UnrealEngineFolderPath.IsNullOrEmpty() || UnrealBuildToolFilePath.IsNullOrEmpty())
        {
            LocateUnrealEngine();
        }
        if (SatisfactoryFolderPath.IsNullOrEmpty() || SatisfactoryExecutableFilePath.IsNullOrEmpty())
        {
            LocateSatisfactorySteam();
        }
        if (ModManagerFolderPath.IsNullOrEmpty() || ModManagerFilePath.IsNullOrEmpty())
        {
            LocateSMM();
        }
        if (UProjectFolderPath.IsNullOrEmpty() || UProjectFilePath.IsNullOrEmpty())
        {
            LocateUProject();
        }
    }

    public void OnNavigatedFrom()  => _localSettingsService.PersistData();

    private void LocateUProject()
    {
        //C:\Users\Mark Lierman\AppData\Local\UnrealEngine\4.26\Saved\Config\Windows\EditorSettings.ini
        var editorSettingsPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\UnrealEngine\\4.26\\Saved\\Config\\Windows\\EditorSettings.ini"; 
        if (File.Exists(editorSettingsPath))
        {
            foreach (var line in File.ReadLines(editorSettingsPath))
            {
                var idx = line.IndexOf("RecentlyOpenedProjectFiles=");
                if (idx >= 0)
                {
                    var projectPath = line.Substring(27, line.Length - 27);
                    projectPath = projectPath.Replace("/", "\\");
                    if (UProjectFilePath.IsNullOrEmpty())
                    {
                        UProjectFilePath = projectPath;
                    }
                    if (UProjectFolderPath.IsNullOrEmpty())
                    {
                        var folderPath = projectPath.Replace("\\FactoryGame.uproject", "");
                        UProjectFolderPath = folderPath;
                    }
                    return;
                }
            }
        }
    }

    private void LocateUnrealEngine()
    {
        //C:\Program Files\Unreal Engine - CSS\Engine\Binaries\DotNET\UnrealBuildTool.exe
        var DefaultCDriveLocation = "C:\\Program Files\\Unreal Engine - CSS\\Engine\\Binaries\\DotNET\\UnrealBuildTool.exe";
        if (File.Exists(DefaultCDriveLocation))
        {
            if (UnrealEngineFolderPath.IsNullOrEmpty())
            {
                UnrealEngineFolderPath = "C:\\Program Files\\Unreal Engine - CSS";
            }
            if (UnrealBuildToolFilePath.IsNullOrEmpty())
            {
                UnrealBuildToolFilePath = DefaultCDriveLocation;
            }
        }
        else
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                var path = Path.Combine(drive.Name, "Program Files\\Unreal Engine - CSS\\Engine\\Binaries\\DotNET\\UnrealBuildTool.exe");
                if (File.Exists(path))
                {
                    if (UnrealEngineFolderPath.IsNullOrEmpty())
                    {
                        UnrealEngineFolderPath = Path.Combine(drive.Name, "Program Files\\Unreal Engine - CSS");
                    }
                    if (UnrealBuildToolFilePath.IsNullOrEmpty())
                    {
                        UnrealBuildToolFilePath = path;
                    }
                    return;
                }
            }
        }
    }

    private void LocateSatisfactorySteam()
    {
        var steamInstallPath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "InstallPath", null);
        steamInstallPath ??= Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam", "SteamPath", null);
        if (steamInstallPath != null)
        {
            var libraryFoldersLines = File.ReadAllLines(steamInstallPath + "\\steamapps\\libraryfolders.vdf");
            foreach (var line in libraryFoldersLines)
            {
                if (line.Contains("path"))
                {
                    var path = line.Substring(11, line.Length - 12);
                    path = path.Replace("\"", "");
                    path = path.Replace("\\\\", "\\");
                    if (Directory.Exists(path + "\\steamapps\\common\\Satisfactory"))
                    {
                        if (SatisfactoryFolderPath.IsNullOrEmpty())
                        {
                            SatisfactoryFolderPath = path + "\\steamapps\\common\\Satisfactory";
                        }
                        if (SatisfactoryExecutableFilePath.IsNullOrEmpty() && File.Exists(path + "\\steamapps\\common\\Satisfactory\\FactoryGame.exe"))
                        {
                            SatisfactoryExecutableFilePath = path + "\\steamapps\\common\\Satisfactory\\FactoryGame.exe";
                        }
                        return;
                    }
                }
            }
        }
    }

    private void LocateSMM()
    {
        var smmPath = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Classes\\smmanager\\shell\\open\\command", null, null);
        if (smmPath != null)
        {
            var smmPathString = smmPath.ToString().Substring(1, smmPath.ToString().Length - 7);
            if (File.Exists(smmPathString ))
            {
                if (ModManagerFolderPath.IsNullOrEmpty())
                {
                    ModManagerFolderPath = smmPathString.Replace("\\Satisfactory Mod Manager.exe","");
                }
                if (ModManagerFilePath.IsNullOrEmpty())
                {
                    ModManagerFilePath = smmPathString;
                }
            }
        }
    }

    private string unrealBuildToolFilePath;
    public string UnrealBuildToolFilePath
    {
        get => unrealBuildToolFilePath;
        set
        {
            SetProperty(ref unrealBuildToolFilePath, value);
            _localSettingsService.Settings.UnrealBuildToolFilePath = value;
            _localSettingsService.PersistData();
        }
    }

    private string unrealEngineFolderPath;
    public string UnrealEngineFolderPath
    {
        get => unrealEngineFolderPath;

        set
        {
            SetProperty(ref unrealEngineFolderPath, value);
            _localSettingsService.Settings.UnrealEngineFolderPath = value;
            _localSettingsService.PersistData();

            if (value != null && _localSettingsService.Settings.UnrealBuildToolFilePath.IsNullOrEmpty())
            {
                if (File.Exists(Path.Combine(value, "FactoryGame.uproject")))
                {
                    _localSettingsService.Settings.UnrealBuildToolFilePath = Path.Combine(value, "FactoryGame.uproject");
                }
            }
        }
    }

    private string uprojectFilePath;
    public string UProjectFilePath
    {
        get => uprojectFilePath;
        set
        {
            SetProperty(ref uprojectFilePath, value);
            _localSettingsService.Settings.UProjectFilePath = value;
            _localSettingsService.PersistData();
        }
    }

    private string uProjectFolderPath;
    public string UProjectFolderPath
    {
        get => uProjectFolderPath;
        set
        {
            SetProperty(ref uProjectFolderPath, value);
            _localSettingsService.Settings.UProjectFolderPath = value;
            _localSettingsService.PersistData();

            if (value != null && _localSettingsService.Settings.UProjectFilePath.IsNullOrEmpty())
            {
                if (File.Exists(Path.Combine(value, "FactoryGame.uproject")))
                {
                    _localSettingsService.Settings.UProjectFilePath = Path.Combine(value, "FactoryGame.uproject");
                }
            }
        }
    }

    private string satisfactoryExecutableFilePath;
    public string SatisfactoryExecutableFilePath
    {
        get => satisfactoryExecutableFilePath;
        set
        {
            SetProperty(ref satisfactoryExecutableFilePath, value);
            _localSettingsService.Settings.SatisfactoryExecutableFilePath = value;
            _localSettingsService.PersistData();
        }
    }

    private string satisfactoryFolderPath;
    public string SatisfactoryFolderPath
    {
        get => satisfactoryFolderPath; set
        {
            SetProperty(ref satisfactoryFolderPath, value);
            _localSettingsService.Settings.SatisfactoryFolderPath = value;
            _localSettingsService.PersistData();

            if (value != null && _localSettingsService.Settings.SatisfactoryExecutableFilePath.IsNullOrEmpty())
            {
                if (File.Exists(Path.Combine(value, "FactoryGame.exe")))
                {
                    _localSettingsService.Settings.SatisfactoryExecutableFilePath = Path.Combine(value, "FactoryGame.exe");
                }
            }
        }
    }

    private string modManagerFolderPath;
    public string ModManagerFolderPath
    {
        get => modManagerFolderPath; set
        {
            SetProperty(ref modManagerFolderPath, value);
            _localSettingsService.Settings.ModManagerFolderPath = value;

            if (value != null && _localSettingsService.Settings.ModManagerFilePath.IsNullOrEmpty())
            {
                if (File.Exists(Path.Combine(value, "Satisfactory Mod Manager.exe")))
                {
                    _localSettingsService.Settings.ModManagerFilePath = Path.Combine(value, "Satisfactory Mod Manager.exe");
                }
            }
            _localSettingsService.PersistData();
        }
    }

    private string modManagerFilePath;
    public string ModManagerFilePath
    {
        get => modManagerFilePath;
        set
        {
            SetProperty(ref modManagerFilePath, value);
            _localSettingsService.Settings.ModManagerFilePath = value;
            _localSettingsService.PersistData();
        }
    }

    private AsyncRelayCommand browseForUELocation;
    public ICommand BrowseForUELocation => browseForUELocation ??= new AsyncRelayCommand(PerformBrowseForUELocation);

    private async Task PerformBrowseForUELocation()
    {
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, ProcessService.GetAppHWND());

        // Use file picker like normal!
        filePicker.FileTypeFilter.Add(".exe");
        var file = await filePicker.PickSingleFileAsync();

        if (file != null)
        {
            UnrealBuildToolFilePath = file.Path;
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
        //    UProjectFolderPath = folderBrowser.FileName;
        //}

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, ProcessService.GetAppHWND());
        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            UProjectFolderPath = folder.Path;
        }
    }

    private AsyncRelayCommand browseForGameLocation;
    public ICommand BrowseForGameLocation => browseForGameLocation ??= new AsyncRelayCommand(PerformBrowseForGameLocation);

    private async Task PerformBrowseForGameLocation()
    {
        //var folderBrowser = new WPFFolderBrowser.WPFFolderBrowserDialog();
        //if (folderBrowser.ShowDialog() == true)
        //{
        //    SatisfactoryFolderPath = folderBrowser.FileName;
        //}

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, ProcessService.GetAppHWND());
        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            SatisfactoryFolderPath = folder.Path;
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
        //    ModManagerFolderPath = folderBrowser.FileName;
        //}

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, ProcessService.GetAppHWND());
        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            ModManagerFolderPath = folder.Path;
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
        //var folderBrowser = new WPFFolderBrowser.WPFFolderBrowserDialog();
        //if (folderBrowser.ShowDialog() == true)
        //{
        //    MPGameLocation = folderBrowser.FileName;
        //}
    }
}
