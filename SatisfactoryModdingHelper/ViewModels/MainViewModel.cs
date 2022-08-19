using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Extensions;
using SatisfactoryModdingHelper.Services;
using SlavaGu.ConsoleAppLauncher;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace SatisfactoryModdingHelper.ViewModels;

public class MainViewModel : ObservableRecipient, INavigationAware
{

    private readonly INavigationService _navigationService;
    private readonly IPluginService _pluginService;
    private readonly ILocalSettingsService _settingsService;
    public readonly IProcessService _processService;
    private string engineLocation = "";
    private string projectLocation = "";
    private string gameLocation = "";
    private string mpGameLocation = "";
    private string modManagerLocation = "";
    private string player1Name = "";
    private string player2Name = "";
    private string player1Args = "";
    private string player2Args = "";
    private bool alpakitCopyMod;
    private bool alpakitCloseGame;

    public MainViewModel(INavigationService navigationService, IPluginService pluginService, ILocalSettingsService settingsService, IProcessService processService)
    {
        _navigationService = navigationService;
        _pluginService = pluginService;
        _settingsService = settingsService;
        _processService=processService;
        PluginComboBoxEnabled = true;
    }

    public void OnNavigatedFrom()
    {
        //Not needed atm
    }

    public void OnNavigatedTo(object parameter)
    {
        projectLocation = _settingsService.Settings.ProjectPath;
        engineLocation = _settingsService.Settings.UnrealEnginePath;
        gameLocation = _settingsService.Settings.SatisfactoryPath;
        player1Name = _settingsService.Settings.Player1Name;
        player2Name = _settingsService.Settings.Player2Name;
        player1Args = _settingsService.Settings.Player1Args;
        player2Args = _settingsService.Settings.Player2Args;
        mpGameLocation = _settingsService.Settings.Player2SatisfactoryPath;
        modManagerLocation = _settingsService.Settings.ModManagerPath;
        SelectedPlugin = _settingsService.Settings.CurrentPlugin;
        alpakitCopyMod = _settingsService.Settings.AlpakitCopyModToGame;
        alpakitCloseGame = _settingsService.Settings.AlpakitCloseGame;
        UpdateOutput();
    }

    public object SelectedPlugin
    {
        get => _pluginService.SelectedPlugin;
        set
        {
            if (value != null)
            {
                _pluginService.SelectedPlugin = value;
                _settingsService.Settings.CurrentPlugin = value.ToString();
                _settingsService.PersistData();
            }
        }
    }

    private bool pluginComboBoxEnabled;
    public bool PluginComboBoxEnabled
    {
        get => pluginComboBoxEnabled; set => SetProperty(ref pluginComboBoxEnabled, value);
    }

    private string outputText;
    public string OutputText
    {
        get => outputText;
        set => SetProperty(ref outputText, value);
    }

    private bool inputsEnabled = true;
    public bool InputsEnabled
    {
        get => inputsEnabled; 
        set => SetProperty(ref inputsEnabled, value);
    }


    private async Task<int> RunBuild(bool isShipping)
    {
        // "C:\Program Files\Unreal Engine - CSS\Engine\Build\BatchFiles\Build.bat" FactoryGame Win64 Shipping -Project="$(SolutionDir)FactoryGame.uproject" -WaitMutex -FromMsBuild
        // "C:\Program Files\Unreal Engine - CSS\Engine\Build\BatchFiles\Build.bat" FactoryGameEditor Win64 Development -Project="$(SolutionDir)FactoryGame.uproject" -WaitMutex -FromMsBuild

        var environmentToBuild = isShipping ? "FactoryGame Win64 Shipping" : "FactoryGameEditor Win64 Development";
        var fileName = @$"`{engineLocation}\Build\BatchFiles\Build.bat`".SetQuotes();
        var cmdLine = @$"{environmentToBuild} -Project=""{projectLocation}\FactoryGame.uproject"" -WaitMutex -FromMsBuild";
        var result = await _processService.RunProcess(@$"`{engineLocation}\Build\BatchFiles\Build.bat`".SetQuotes(), @$"{environmentToBuild} -Project=""{projectLocation}\FactoryGame.uproject"" -WaitMutex -FromMsBuild");
        return result;
    }

    public async void UpdateOutput()
    {
        while (true)
        {
            OutputText = _processService.OutputText;
            await Task.Delay(500);

            // Highlighting regex wip
            // ^\s*(?'ProgressGroup'\[\d+\/\d+\].*$)|^.*\):\s(?'InfoType'\w+).*(?'CodeReference''.*'):\s(?'Message'.*$)
        }
    }

    public void RunApp(string fileName, string cmdLine, Action<string> replyHandler)
    {
        var app = new ConsoleApp(fileName, cmdLine);
        app.ConsoleOutput += (o, args) =>
        {
            replyHandler(args.Line);
        };
        app.Run();
        app.WaitForExit();
    }

    private AsyncRelayCommand buildForDevelopmentEditor;
    public ICommand BuildForDevelopmentEditor => buildForDevelopmentEditor ??= new AsyncRelayCommand(PerformBuildForDevelopmentEditor);
    private async Task PerformBuildForDevelopmentEditor()
    {
        InputsEnabled = false;
        _processService.OutputText = "Building Development Editor..." + Environment.NewLine;
        var exitCode = await RunBuild(false);
        _processService.SendProcessFinishedMessage(exitCode, "Build for Development Editor");
        InputsEnabled = true;
    }

    private AsyncRelayCommand buildForShipping;
    public ICommand BuildForShipping => buildForShipping ??= new AsyncRelayCommand(PerformBuildForShipping);
    private async Task PerformBuildForShipping()
    {
        _processService.OutputText = "Building Shipping..." + Environment.NewLine;
        var exitCode = await RunBuild(true);
        _processService.SendProcessFinishedMessage(exitCode, "Build for Shipping");
    }

    private AsyncRelayCommand launchSatisfactory;
    public ICommand LaunchSatisfactory => launchSatisfactory ??= new AsyncRelayCommand(PerformLaunchSatisfactory);
    private Task PerformLaunchSatisfactory()
    {
        _processService.OutputText = "Launching Satisfactory...";
        _=_processService.RunProcess(@$"{gameLocation}\FactoryGame.exe", "", false);
        return Task.CompletedTask;
    }

    private AsyncRelayCommand launchModManager;
    public ICommand LaunchModManager => launchModManager ??= new AsyncRelayCommand(PerformLaunchModManager);
    private Task PerformLaunchModManager()
    {
        _processService.OutputText = "Launching Satisfactory Mod Manager...";
        _=_processService.RunProcess(@$"{modManagerLocation}\Satisfactory Mod Manager.exe", "", false);
        return Task.CompletedTask;
    }

    private AsyncRelayCommand<bool> runAlpakit;
    public ICommand RunAlpakit => runAlpakit ??= new AsyncRelayCommand<bool>(PerformRunAlpakit);
    private async Task PerformRunAlpakit(bool launchGame)
    {
        if (alpakitCloseGame)
        {
            _processService.CloseRunningSatisfactoryProcesses();
        }

        _processService.OutputText = "Running Alpakit..." + Environment.NewLine;
        var alpakitArgs = alpakitCopyMod ? @$" -CopyToGameDir -GameDir=`{gameLocation}`" : "";
        var exitCode = await _processService.RunProcess(@$"{engineLocation}\Build\BatchFiles\RunUAT.bat", $@" -ScriptsForProject=`{projectLocation}\FactoryGame.uproject` PackagePlugin -Project=`{projectLocation}\FactoryGame.uproject` -PluginName=`{SelectedPlugin}` {alpakitArgs}".SetQuotes());
        _processService.SendProcessFinishedMessage(exitCode, "Alpakit");

        if (exitCode == 0 && launchGame)
        {
            _=PerformLaunchSatisfactory();
        }
    }

    private AsyncRelayCommand launchMPTesting;
    public ICommand LaunchMPTesting => launchMPTesting ??= new AsyncRelayCommand(PerformLaunchMPTesting);
    private async Task PerformLaunchMPTesting()
    {
        //Build launch strings
        var launchStringArgs1 = $"-EpicPortal -NoSteamClient -Username=`{player1Name}` {player1Args}".SetQuotes();
        var launchStringArgs2 = $"-EpicPortal -NoSteamClient -Username=`{player2Name}` {player2Args}".SetQuotes();

        _=_processService.RunProcess(@$"`{gameLocation}\FactoryGame.exe`".SetQuotes(), launchStringArgs1, false);

        Thread.Sleep(1000); // Wait a second before launching 2nd game instance
        if (mpGameLocation.Length > 0)
        {
            await MirrorInstallForMPTest();
            _=_processService.RunProcess(@$"`{mpGameLocation}\FactoryGame.exe`".SetQuotes(), launchStringArgs2, false);
        }
        else
        {
            _=_processService.RunProcess(@$"`{gameLocation}\FactoryGame.exe`".SetQuotes(), launchStringArgs2, false);
        }
    }

    private async Task MirrorInstallForMPTest()
    {
        _processService.OutputText += $"Mirroring Satisfactory install to secondary location...{Environment.NewLine}";
        var launchStringArgs = @$"`{gameLocation}` `{mpGameLocation}` /PURGE /MIR /XD Configs /R:2 /W:2 /NS /NDL /NFL /NP";
        await _processService.RunProcess(@$"`ROBOCOPY.EXE`".SetQuotes(), launchStringArgs.SetQuotes(), false);
    }

    private bool aIOBuildDevEditor;
    public bool AIOBuildDevEditor
    {
        get => aIOBuildDevEditor; set => SetProperty(ref aIOBuildDevEditor, value);
    }

    private bool aIOBuildShipping;
    public bool AIOBuildShipping
    {
        get => aIOBuildShipping; set => SetProperty(ref aIOBuildShipping, value);
    }

    private bool aIOAlpakit;
    public bool AIOAlpakit
    {
        get => aIOAlpakit; set => SetProperty(ref aIOAlpakit, value);
    }

    private bool aIOLaunchGame;
    public bool AIOLaunchGame
    {
        get => aIOLaunchGame; set => SetProperty(ref aIOLaunchGame, value);
    }

    private bool aIOLaunchMP;
    public bool AIOLaunchMP
    {
        get => aIOLaunchMP; set => SetProperty(ref aIOLaunchMP, value);
    }

    private AsyncRelayCommand runAllChecked;
    public ICommand RunAllChecked => runAllChecked ??= new AsyncRelayCommand(PerformRunAllChecked);

    private async Task PerformRunAllChecked()
    {
        if (AIOBuildDevEditor)
        {
            await PerformBuildForDevelopmentEditor();
        }
        if (AIOBuildShipping)
        {
            await PerformBuildForShipping();
        }
        if (AIOAlpakit)
        {
            await PerformRunAlpakit(false);
        }
        if (!AIOLaunchMP && AIOLaunchGame)
        {
            await PerformLaunchSatisfactory();
        }
        if (AIOLaunchMP)
        {
            await PerformLaunchMPTesting();
        }
    }
}
