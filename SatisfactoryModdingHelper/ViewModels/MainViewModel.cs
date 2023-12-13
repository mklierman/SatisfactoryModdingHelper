using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Extensions;
using SatisfactoryModdingHelper.Helpers;
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
    private string? mpGameLocation = "";
  //  private string modManagerLocation = "";
    private string? player1Name = "";
    private string? player2Name = "";
    private string? player1Args = "";
    private string? player2Args = "";
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

    private ObservableCollection<string> outputText = new();
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

        var environmentToBuild = isShipping ? StringHelper.WinShipping : StringHelper.WinDev;
        var fileName = StringHelper.GetBuildBatPath(_settingsService.Settings.UnrealEngineFolderPath);
        var args = StringHelper.GetBuildArgs(environmentToBuild, _settingsService.Settings.UProjectFilePath);
        var result = await _processService.RunProcess(fileName, args);

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
        _processService.OutputText = StringHelper.BuildingDev + Environment.NewLine;
        var exitCode = await RunBuild(false);
        _processService.SendProcessFinishedMessage(exitCode, StringHelper.BuildForDev);
        _appNotificationService.SendNotification(StringHelper.BuildDevComplete);
    }

    private AsyncRelayCommand buildForShipping;
    public ICommand BuildForShipping => buildForShipping ??= new AsyncRelayCommand(PerformBuildForShipping);
    private async Task PerformBuildForShipping()
    {
        _processService.OutputText = StringHelper.BuildingShipping + Environment.NewLine;
        var exitCode = await RunBuild(true);
        _processService.SendProcessFinishedMessage(exitCode, StringHelper.BuildForShipping);
        _appNotificationService.SendNotification(StringHelper.BuildShippingComplete);
    }

    private AsyncRelayCommand launchSatisfactory;
    public ICommand LaunchSatisfactory => launchSatisfactory ??= new AsyncRelayCommand(PerformLaunchSatisfactory);
    private Task PerformLaunchSatisfactory()
    {
        _processService.OutputText = StringHelper.LaunchingSatisfactory;
        _ =_processService.RunProcess(@$"{_settingsService.Settings.SatisfactoryExecutableFilePath}", "", false);
        return Task.CompletedTask;
    }

    private AsyncRelayCommand launchModManager;
    public ICommand LaunchModManager => launchModManager ??= new AsyncRelayCommand(PerformLaunchModManager);
    private Task PerformLaunchModManager()
    {
        _processService.OutputText = StringHelper.LaunchingSMM;
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

        _processService.OutputText = StringHelper.RunningAlpakit + Environment.NewLine;

        var exitCode = await _processService.RunProcess(StringHelper.GetUATBatPath(_settingsService.Settings.UnrealEngineFolderPath),
            StringHelper.GetAlpakitArgs(alpakitCopyMod, _settingsService.Settings.SatisfactoryFolderPath, _settingsService.Settings.UProjectFilePath, SelectedPlugin.ToString()));

        _processService.SendProcessFinishedMessage(exitCode, "Alpakit");

        _appNotificationService.SendNotification(StringHelper.AlpakitComplete);

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
    public ICommand LaunchMPHost => launchMPHost ??= new AsyncRelayCommand(PerformLaunchMPHost);
    private async Task PerformLaunchMPHost()
    {
        var launchStringArgs = StringHelper.GetMPLaunchArgs(player1Args);
        _ = _processService.RunProcess(@$"`{_settingsService.Settings.SatisfactoryExecutableFilePath}`".SetQuotes(), launchStringArgs, false);
        _processService.AddStringToOutput(StringHelper.LaunchingMPHost);
    }

    private AsyncRelayCommand launchMPClient;
    public ICommand LaunchMPClient => launchMPClient ??= new AsyncRelayCommand(PerformLaunchMPClient);
    private async Task PerformLaunchMPClient()
    {
        var launchStringArgs = StringHelper.GetMPLaunchArgs(player2Args);

        if (mpGameLocation?.Length > 0)
        {
            await MirrorInstallForMPTest();
            _ = _processService.RunProcess($"\"{mpGameLocation}\\FactoryGame.exe\"", launchStringArgs, false);
        }
        else
        {
            _ = _processService.RunProcess($"\"{_settingsService.Settings.SatisfactoryExecutableFilePath}\"", launchStringArgs, false);
        }
        _processService.AddStringToOutput(StringHelper.LaunchingMPClient);
    }

    private async Task MirrorInstallForMPTest()
    {
        _processService.AddStringToOutput(StringHelper.RunningMirror);
        
        var launchStringArgs = StringHelper.GetRobocopyArgs(_settingsService.Settings.SatisfactoryFolderPath, mpGameLocation);
        await _processService.RunProcess("\"ROBOCOPY.EXE\"", launchStringArgs, false);
        _processService.AddStringToOutput(StringHelper.MirrorComplete);
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
