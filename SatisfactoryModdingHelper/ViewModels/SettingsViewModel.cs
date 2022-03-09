using System;
using System.IO;
using System.Windows.Input;

using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Models;


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

        public ICommand SetThemeCommand => _setThemeCommand ?? (_setThemeCommand = new RelayCommand<string>(OnSetTheme));

        public ICommand PrivacyStatementCommand => _privacyStatementCommand ?? (_privacyStatementCommand = new RelayCommand(OnPrivacyStatement));

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
            UnrealEngineLocation = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_UE);
            VisualStudioLocation = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_VS);
            ProjectFolder = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_Project);
            SatisfactoryFolder = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_Satisfactory);
            SMMFolder = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_SMM);
            AlpakitCopyPlugin = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Alpakit_CopyMod);
            AlpakitCloseSatisfactory = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Alpakit_CloseGame);
            AlpakitSteam = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Alpakit_Steam);
            AlpakitEGS = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Alpakit_EGS);
            AlpakitEGSExperimental = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Alpakit_EGS_Exp);

        }

        public void OnNavigatedFrom()
        {
            _persistAndRestoreService.PersistData();
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
                _persistAndRestoreService.SaveProperty(Properties.Resources.Settings_Locations_UE, value);
            }
        }

        private string visualStudioLocation;

        public string VisualStudioLocation
        {
            get => visualStudioLocation;
            set
            {
                SetProperty(ref visualStudioLocation, value);
                _persistAndRestoreService.SaveProperty(Properties.Resources.Settings_Locations_VS, value);
            }
        }

        private string projectFolder;

        public string ProjectFolder
        {
            get => projectFolder;
            set
            {
                SetProperty(ref projectFolder, value);
                _persistAndRestoreService.SaveProperty(Properties.Resources.Settings_Locations_Project, value);
            }
        }

        private string satisfactoryFolder;

        public string SatisfactoryFolder
        {
            get => satisfactoryFolder; set
            {
                SetProperty(ref satisfactoryFolder, value);
                _persistAndRestoreService.SaveProperty(Properties.Resources.Settings_Locations_Satisfactory, value);
            }
        }

        private string sMMFolder;

        public string SMMFolder
        {
            get => sMMFolder; set
            {
                SetProperty(ref sMMFolder, value);
                _persistAndRestoreService.SaveProperty(Properties.Resources.Settings_Locations_SMM, value);
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

        private bool? alpakitCopyPlugin;

        public bool? AlpakitCopyPlugin
        {
            get => alpakitCopyPlugin;
            set
            {
                if (value == null)
                {
                    value = false;
                }

                SetProperty(ref alpakitCopyPlugin, value);
                _persistAndRestoreService.SaveProperty(Properties.Resources.Settings_Alpakit_CopyMod, value);
            }
        }

        private bool? alpakitCloseSatisfactory;

        public bool? AlpakitCloseSatisfactory
        {
            get => alpakitCloseSatisfactory;
            set
            {
                if (value == null)
                {
                    value = false;
                }
                SetProperty(ref alpakitCloseSatisfactory, value);
                _persistAndRestoreService.SaveProperty(Properties.Resources.Settings_Alpakit_CloseGame, value);
            }
        }

        private bool? alpakitSteam;

        public bool? AlpakitSteam
        {
            get => alpakitSteam;
            set
            {
                if (value == null)
                {
                    value = false;
                }
                SetProperty(ref alpakitSteam, value);
                _persistAndRestoreService.SaveProperty(Properties.Resources.Settings_Alpakit_Steam, value);
            }
        }

        private bool? alpakitEGS;

        public bool? AlpakitEGS
        {
            get => alpakitEGS;
            set
            {
                if (value == null)
                {
                    value = false;
                }
                SetProperty(ref alpakitEGS, value);
                _persistAndRestoreService.SaveProperty(Properties.Resources.Settings_Alpakit_EGS, value);
            }
        }

        private bool? alpakitEGSExperimental;

        public bool? AlpakitEGSExperimental
        {
            get => alpakitEGSExperimental;
            set
            {
                if (value == null)
                {
                    value = false;
                }
                SetProperty(ref alpakitEGSExperimental, value);
                _persistAndRestoreService.SaveProperty(Properties.Resources.Settings_Alpakit_EGS_Exp, value);
            }
        }

        private string mPPlayer1Name;

        public string MPPlayer1Name
        {
            get => mPPlayer1Name;
            set
            {
                SetProperty(ref mPPlayer1Name, value);
                _persistAndRestoreService.SaveProperty(Properties.Resources.Settings_MP_Player1Name, value);
            }
        }

        private string mPPlayer2Name;

        public string MPPlayer2Name
        {
            get => mPPlayer2Name;
            set
            {
                SetProperty(ref mPPlayer2Name, value);
                _persistAndRestoreService.SaveProperty(Properties.Resources.Settings_MP_Player2Name, value);
            }
        }
}
}
