using System;
using System.IO;
using System.Windows.Input;

using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Models;
using SatisfactoryModdingHelper.Services;


namespace SatisfactoryModdingHelper.ViewModels
{
    // TODO WTS: Change the URL for your privacy policy in the appsettings.json file, currently set to https://YourPrivacyUrlGoesHere
    public class SettingsViewModel : ObservableObject, INavigationAware
    {
        private readonly AppConfig _appConfig;
        private readonly IThemeSelectorService _themeSelectorService;
        private readonly ISystemService _systemService;
        private readonly IApplicationInfoService _applicationInfoService;
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private AppTheme _theme;
        private string _versionDescription;
        private ICommand _setThemeCommand;
        private ICommand _privacyStatementCommand;

        public AppTheme Theme
        {
            get { return _theme; }
            set { SetProperty(ref _theme, value); }
        }

        public string VersionDescription
        {
            get { return _versionDescription; }
            set { SetProperty(ref _versionDescription, value); }
        }

        public ICommand SetThemeCommand => _setThemeCommand ??= new RelayCommand<string>(OnSetTheme);

        public ICommand PrivacyStatementCommand => _privacyStatementCommand ??= new RelayCommand(OnPrivacyStatement);

        public SettingsViewModel(IOptions<AppConfig> appConfig, IThemeSelectorService themeSelectorService, ISystemService systemService, IApplicationInfoService applicationInfoService, IPersistAndRestoreService persistAndRestoreService)
        {
            _appConfig = appConfig.Value;
            _themeSelectorService = themeSelectorService;
            _systemService = systemService;
            _applicationInfoService = applicationInfoService;
            _persistAndRestoreService = persistAndRestoreService;
        }

        public void OnNavigatedTo(object parameter)
        {
            VersionDescription = $"{Properties.Resources.AppDisplayName} - {_applicationInfoService.GetVersion()}";
            Theme = _themeSelectorService.GetCurrentTheme();
            UnrealEngineLocation = _persistAndRestoreService.Settings.UnrealEnginePath;
            VisualStudioLocation = _persistAndRestoreService.Settings.VisualStudioPath;
            ProjectFolder = _persistAndRestoreService.Settings.ProjectPath;
            SatisfactoryFolder = _persistAndRestoreService.Settings.SatisfactoryPath;
            SMMFolder = _persistAndRestoreService.Settings.ModManagerPath;
            AlpakitCopyPlugin = _persistAndRestoreService.Settings.AlpakitCopyModToGame;
            AlpakitCloseSatisfactory = _persistAndRestoreService.Settings.AlpakitCloseGame;
            MPPlayer1Name = _persistAndRestoreService.Settings.Player1Name;
            MPPlayer2Name = _persistAndRestoreService.Settings.Player2Name;
            MPArgs1 = _persistAndRestoreService.Settings.Player1Args;
            MPArgs2 = _persistAndRestoreService.Settings.Player2Args;
            MPGameLocation = _persistAndRestoreService.Settings.Player2SatisfactoryPath;

        }

        public void OnNavigatedFrom()
        {
            _persistAndRestoreService.PersistData();
        }

        public void OnStartingNavigateFrom()
        {
        }

        private void OnSetTheme(string themeName)
        {
            var theme = (AppTheme)Enum.Parse(typeof(AppTheme), themeName);
            _themeSelectorService.SetTheme(theme);
        }

        private void OnPrivacyStatement()
            => _systemService.OpenInWebBrowser(_appConfig.PrivacyStatement);

        private string unrealEngineLocation;

        public string UnrealEngineLocation
        {
            get => unrealEngineLocation;

            set
            {
                SetProperty(ref unrealEngineLocation, value);
                _persistAndRestoreService.Settings.UnrealEnginePath = value;
                _persistAndRestoreService.PersistData();
            }
        }

        private string visualStudioLocation;

        public string VisualStudioLocation
        {
            get => visualStudioLocation;
            set
            {
                SetProperty(ref visualStudioLocation, value);
                _persistAndRestoreService.Settings.VisualStudioPath = value;
                _persistAndRestoreService.PersistData();
            }
        }

        private string projectFolder;

        public string ProjectFolder
        {
            get => projectFolder;
            set
            {
                SetProperty(ref projectFolder, value);
                _persistAndRestoreService.Settings.ProjectPath = value;
                _persistAndRestoreService.PersistData();
            }
        }

        private string satisfactoryFolder;

        public string SatisfactoryFolder
        {
            get => satisfactoryFolder; set
            {
                SetProperty(ref satisfactoryFolder, value);
                _persistAndRestoreService.Settings.SatisfactoryPath = value;
                _persistAndRestoreService.PersistData();
            }
        }

        private string sMMFolder;

        public string SMMFolder
        {
            get => sMMFolder; set
            {
                SetProperty(ref sMMFolder, value);
                _persistAndRestoreService.Settings.ModManagerPath = value;
                _persistAndRestoreService.PersistData();
            }
        }

        private RelayCommand browseForUELocation;
        public ICommand BrowseForUELocation => browseForUELocation ??= new RelayCommand(PerformBrowseForUELocation);

        private void PerformBrowseForUELocation()
        {
            var folderBrowser = new WPFFolderBrowser.WPFFolderBrowserDialog();
            folderBrowser.InitialDirectory = Properties.Resources.Settings_Locations_UE_Hint.ToString();
            if (folderBrowser.ShowDialog() == true)
            {
                UnrealEngineLocation = folderBrowser.FileName;
            }
        }

        private RelayCommand browseForVSLocation;
        public ICommand BrowseForVSLocation => browseForVSLocation ??= new RelayCommand(PerformBrowseForVSLocation);

        private void PerformBrowseForVSLocation()
        {
            var folderBrowser = new WPFFolderBrowser.WPFFolderBrowserDialog();
            var vs2019 = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE";
            var vs2017 = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE";
            var vs2015 = @"C:\Program Files (x86)\Microsoft Visual Studio\2015\Community\Common7\IDE";
            if (Directory.Exists(vs2019))
            {
                folderBrowser.InitialDirectory = vs2019;
            }
            else if (Directory.Exists(vs2017))
            {
                folderBrowser.InitialDirectory = vs2017;
            }
            else
            {
                folderBrowser.InitialDirectory = vs2015;
            }
            if (folderBrowser.ShowDialog() == true)
            {
                VisualStudioLocation = folderBrowser.FileName;
            }
        }

        private RelayCommand browseForProjectLocation;
        public ICommand BrowseForProjectLocation => browseForProjectLocation ??= new RelayCommand(PerformBrowseForProjectLocation);

        private void PerformBrowseForProjectLocation()
        {
            var folderBrowser = new WPFFolderBrowser.WPFFolderBrowserDialog();
            folderBrowser.InitialDirectory = "C:\\";
            if (folderBrowser.ShowDialog() == true)
            {
                ProjectFolder = folderBrowser.FileName;
            }
        }

        private RelayCommand browseForGameLocation;
        public ICommand BrowseForGameLocation => browseForGameLocation ??= new RelayCommand(PerformBrowseForGameLocation);

        private void PerformBrowseForGameLocation()
        {
            var folderBrowser = new WPFFolderBrowser.WPFFolderBrowserDialog();
            if (folderBrowser.ShowDialog() == true)
            {
                SatisfactoryFolder = folderBrowser.FileName;
            }
        }

        private RelayCommand browseForSMMLocation;
        public ICommand BrowseForSMMLocation => browseForSMMLocation ??= new RelayCommand(PerformBrowseForSMMLocation);

        private void PerformBrowseForSMMLocation()
        {
            var folderBrowser = new WPFFolderBrowser.WPFFolderBrowserDialog();
            folderBrowser.InitialDirectory = $"{Environment.GetEnvironmentVariable("LocalAppData")}\\Programs\\Satisfactory Mod Manager";
            if (folderBrowser.ShowDialog() == true)
            {
                SMMFolder = folderBrowser.FileName;
            }
        }

        private bool alpakitCopyPlugin;

        public bool AlpakitCopyPlugin
        {
            get => alpakitCopyPlugin;
            set
            {
                SetProperty(ref alpakitCopyPlugin, value);
                _persistAndRestoreService.Settings.AlpakitCopyModToGame = value;
                _persistAndRestoreService.PersistData();
            }
        }

        private bool alpakitCloseSatisfactory;

        public bool AlpakitCloseSatisfactory
        {
            get => alpakitCloseSatisfactory;
            set
            {
                SetProperty(ref alpakitCloseSatisfactory, value);
                _persistAndRestoreService.Settings.AlpakitCloseGame = value;
                _persistAndRestoreService.PersistData();
            }
        }

        private string mPPlayer1Name;

        public string MPPlayer1Name
        {
            get => mPPlayer1Name;
            set
            {
                SetProperty(ref mPPlayer1Name, value);
                _persistAndRestoreService.Settings.Player1Name = value;
                _persistAndRestoreService.PersistData();
            }
        }

        private string mPPlayer2Name;

        public string MPPlayer2Name
        {
            get => mPPlayer2Name;
            set
            {
                SetProperty(ref mPPlayer2Name, value);
                _persistAndRestoreService.Settings.Player2Name = value;
                _persistAndRestoreService.PersistData();
            }
        }

        private string mPArgs1;

        public string MPArgs1
        {
            get => mPArgs1;
            set
            {
                SetProperty(ref mPArgs1, value);
                _persistAndRestoreService.Settings.Player1Args = value;
                _persistAndRestoreService.PersistData();
            }
        }

        private string mPArgs2;

        public string MPArgs2
        {
            get => mPArgs2;
            set
            {
                SetProperty(ref mPArgs2, value);
                _persistAndRestoreService.Settings.Player2Args = value;
                _persistAndRestoreService.PersistData();
            }
        }

        private string mPGameLocation;
        public string MPGameLocation
        {
            get => mPGameLocation;
            set
            {
                SetProperty(ref mPGameLocation, value);
                _persistAndRestoreService.Settings.Player2SatisfactoryPath = value;
                _persistAndRestoreService.PersistData();
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
}
