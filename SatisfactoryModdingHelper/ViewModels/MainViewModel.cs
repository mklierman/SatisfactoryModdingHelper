﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Extensions;
using SatisfactoryModdingHelper.Notifications;
using SatisfactoryModdingHelper.Services;
using SlavaGu.ConsoleAppLauncher;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace SatisfactoryModdingHelper.ViewModels;

public class MainViewModel : ObservableRecipient, INavigationAware
{

    private readonly INavigationService _navigationService;
    public readonly IPluginService _pluginService;
    private readonly ILocalSettingsService _settingsService;
    public readonly IProcessService _processService;
    private readonly IAppNotificationService _appNotificationService;
    //private string engineLocation = "";
    //private string projectLocation = "";
    //private string gameLocation = "";
    private string mpGameLocation = "";
  //  private string modManagerLocation = "";
    private string player1Name = "";
    private string player2Name = "";
    private string player1Args = "";
    private string player2Args = "";
    private bool alpakitCopyMod = true;
    private bool alpakitCloseGame;

    public MainViewModel(INavigationService navigationService, IPluginService pluginService, ILocalSettingsService settingsService, IProcessService processService, IAppNotificationService appNotificationService)
    {
        _navigationService = navigationService;
        _pluginService = pluginService;
        _settingsService = settingsService;
        _processService=processService;
        _appNotificationService=appNotificationService;
        PluginComboBoxEnabled = true;
    }

    public void OnNavigatedFrom()
    {
        RunUpdateOutput = false;
    }

    public void OnNavigatedTo(object parameter)
    {
      //  projectLocation = _settingsService.Settings.UProjectFolderPath;
       // engineLocation = _settingsService.Settings.UnrealEngineFolderPath;
       // gameLocation = _settingsService.Settings.SatisfactoryFolderPath;
        player1Name = _settingsService.Settings.Player1Name;
        player2Name = _settingsService.Settings.Player2Name;
        player1Args = _settingsService.Settings.Player1Args;
        player2Args = _settingsService.Settings.Player2Args;
        mpGameLocation = _settingsService.Settings.Player2SatisfactoryPath;
      //  modManagerLocation = _settingsService.Settings.ModManagerFolderPath;
        SelectedPlugin = _settingsService.Settings.CurrentPlugin;
       // alpakitCopyMod = _settingsService.Settings.AlpakitCopyModToGame;
        alpakitCloseGame = _settingsService.Settings.AlpakitCloseGame;
        RunUpdateOutput = true;
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

    private ObservableCollection<string> outputText = new ObservableCollection<string>();
    public ObservableCollection<string> OutputText
    {
        get => outputText;
        set => SetProperty(ref outputText, value);
    }

    private long lastFileLocation = 0;
    private bool RunUpdateOutput = false;

    public async void UpdateOutput()
    {
        var path = Path.GetDirectoryName(Environment.ProcessPath) + "\\ProcessLog.txt";

        while (RunUpdateOutput)
        {
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                StreamReader sr = new StreamReader(fs);

                var initialFileSize = fs.Length;
                var newLength = initialFileSize - lastFileLocation;
                if (newLength > 0)
                {
                    fs.Seek(lastFileLocation, SeekOrigin.Begin);
                    while (!sr.EndOfStream)
                    {
                        var newline = sr.ReadLine();
                        if (newline != null)
                        {
                            OutputText.Add(newline);
                        }
                    }
                    lastFileLocation = fs.Position;
                }
            }
            //InputsEnabled = !_processService.ProcessRunning;
            await Task.Delay(500);

            // Highlighting regex wip
            // ^\s*(?'ProgressGroup'\[\d+\/\d+\].*$)|^.*\):\s(?'InfoType'\w+).*(?'CodeReference''.*'):\s(?'Message'.*$)
        }
    }

    private bool inputsEnabled = true;
    public bool InputsEnabled
    {
        get => true;
        set => SetProperty(ref inputsEnabled, true);
    }


    private async Task<int> RunBuild(bool isShipping)
    {
        // Sample commands
        // "C:\Program Files\Unreal Engine - CSS\Engine\Build\BatchFiles\Build.bat" FactoryGame Win64 Shipping -Project="$(SolutionDir)FactoryGame.uproject" -WaitMutex -FromMsBuild
        // "C:\Program Files\Unreal Engine - CSS\Engine\Build\BatchFiles\Build.bat" FactoryGameEditor Win64 Development -Project="$(SolutionDir)FactoryGame.uproject" -WaitMutex -FromMsBuild

        var environmentToBuild = isShipping ? "FactoryGame Win64 Shipping" : "FactoryGameEditor Win64 Development";
        var fileName = @$"`{_settingsService.Settings.UnrealEngineFolderPath}\Engine\Build\BatchFiles\Build.bat`".SetQuotes();
        var cmdLine = @$"{environmentToBuild} -Project=""{_settingsService.Settings.UProjectFilePath}"" -WaitMutex -FromMsBuild";
        var result = await _processService.RunProcess(fileName, cmdLine);

        return result;
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
        _processService.OutputText = "Building Development Editor..." + Environment.NewLine;
        var exitCode = await RunBuild(false);
        _processService.SendProcessFinishedMessage(exitCode, "Build for Development Editor");
        _appNotificationService.SendNotification($"Build for Development Editor Complete");
    }

    private AsyncRelayCommand buildForShipping;
    public ICommand BuildForShipping => buildForShipping ??= new AsyncRelayCommand(PerformBuildForShipping);
    private async Task PerformBuildForShipping()
    {
        _processService.OutputText = "Building Shipping..." + Environment.NewLine;
        var exitCode = await RunBuild(true);
        _processService.SendProcessFinishedMessage(exitCode, "Build for Shipping");
        _appNotificationService.SendNotification($"Build for Shipping Complete");
    }

    private AsyncRelayCommand launchSatisfactory;
    public ICommand LaunchSatisfactory => launchSatisfactory ??= new AsyncRelayCommand(PerformLaunchSatisfactory);
    private Task PerformLaunchSatisfactory()
    {
        _processService.OutputText = "Launching Satisfactory...";
        _=_processService.RunProcess(@$"{_settingsService.Settings.SatisfactoryExecutableFilePath}", "", false);
        return Task.CompletedTask;
    }

    private AsyncRelayCommand launchModManager;
    public ICommand LaunchModManager => launchModManager ??= new AsyncRelayCommand(PerformLaunchModManager);
    private Task PerformLaunchModManager()
    {
        _processService.OutputText = "Launching Satisfactory Mod Manager...";
        _=_processService.RunProcess(@$"{_settingsService.Settings.ModManagerFilePath}", "", false);
        return Task.CompletedTask;
    }

    private AsyncRelayCommand<string> runAlpakit;
    public ICommand RunAlpakit => runAlpakit ??= new AsyncRelayCommand<string>(PerformRunAlpakit);
    private async Task PerformRunAlpakit(string launchGame)
    {
        if (alpakitCloseGame)
        {
            _processService.CloseRunningSatisfactoryProcesses();
        }

        _processService.OutputText = "Running Alpakit..." + Environment.NewLine;
        var alpakitArgs = alpakitCopyMod ? @$" -CopyToGameDir -GameDir=`{_settingsService.Settings.SatisfactoryFolderPath}`" : "";
        var exitCode = await _processService.RunProcess(@$"{_settingsService.Settings.UnrealEngineFolderPath}\Engine\Build\BatchFiles\RunUAT.bat",
            $@" -ScriptsForProject=`{_settingsService.Settings.UProjectFilePath}` PackagePlugin -Project=`{_settingsService.Settings.UProjectFilePath}` -PluginName=`{SelectedPlugin}` {alpakitArgs}".SetQuotes());
        _processService.SendProcessFinishedMessage(exitCode, "Alpakit");
        _appNotificationService.SendNotification($"Alpakit Complete");
        if (exitCode == 0 && launchGame == "True")
        {
            _=PerformLaunchSatisfactory();
        }
    }

    private AsyncRelayCommand launchMPTesting;
    public ICommand LaunchMPTesting => launchMPTesting ??= new AsyncRelayCommand(PerformLaunchMPTesting);
    private async Task PerformLaunchMPTesting()
    {
        PerformLaunchMPHost();
        await Task.Delay(1000);
        PerformLaunchMPClient();
    }

    private AsyncRelayCommand launchMPHost;
    public ICommand LaunchMPHost => launchMPTesting ??= new AsyncRelayCommand(PerformLaunchMPHost);
    private async Task PerformLaunchMPHost()
    {
        var launchStringArgs1 = $"-EpicPortal -NoSteamClient {player1Args}".SetQuotes();
        _ = _processService.RunProcess(@$"`{_settingsService.Settings.SatisfactoryExecutableFilePath}`".SetQuotes(), launchStringArgs1, false);
        _processService.AddStringToOutput($"Launching MP Host");
    }

    private AsyncRelayCommand launchMPClient;
    public ICommand LaunchMPClient => launchMPTesting ??= new AsyncRelayCommand(PerformLaunchMPClient);
    private async Task PerformLaunchMPClient()
    {
        var launchStringArgs2 = $"-EpicPortal -NoSteamClient {player2Args}".SetQuotes();

        if (mpGameLocation?.Length > 0)
        {
            await MirrorInstallForMPTest();
            _ = _processService.RunProcess(@$"`{mpGameLocation}\FactoryGame.exe`".SetQuotes(), launchStringArgs2, false);
            _processService.AddStringToOutput($"Launching MP Client");
        }
        else
        {
            _ = _processService.RunProcess(@$"`{_settingsService.Settings.SatisfactoryExecutableFilePath}`".SetQuotes(), launchStringArgs2, false);
            _processService.AddStringToOutput($"Launching MP Client");
        }
    }

    private async Task MirrorInstallForMPTest()
    {
        _processService.AddStringToOutput($"Mirroring Satisfactory install to secondary location...");
        var launchStringArgs = @$"`{_settingsService.Settings.SatisfactoryFolderPath}` `{mpGameLocation}` /PURGE /MIR /XD Configs /R:2 /W:2 /NS /NDL /NFL /NP";
        await _processService.RunProcess(@$"`ROBOCOPY.EXE`".SetQuotes(), launchStringArgs.SetQuotes(), false);
        _processService.AddStringToOutput($"Mirroring Complete");
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
            await PerformRunAlpakit("False");
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

    private AsyncRelayCommand clearOutput;
    public ICommand ClearOutput => clearOutput ??= new AsyncRelayCommand(PerformClearOutput);
    private async Task PerformClearOutput()
    {
        var path = Path.GetDirectoryName(Environment.ProcessPath) + "\\ProcessLog.txt";
        File.WriteAllText(path, "");
        OutputText.Clear();
    }
}
