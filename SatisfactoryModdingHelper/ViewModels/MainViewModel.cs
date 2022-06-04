using System;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using SatisfactoryModdingHelper.Models;
using Newtonsoft.Json;
using ControlzEx.Standard;
using System.Threading;
using System.Runtime.InteropServices;
using SatisfactoryModdingHelper.Extensions;
using SatisfactoryModdingHelper.Services;
using System.Collections.ObjectModel;

namespace SatisfactoryModdingHelper.ViewModels
{
    public class MainViewModel : ObservableObject, INavigationAware
    {
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private readonly INavigationService _navigationService;
        private readonly IPluginService _pluginService;
        private string engineLocation;
        private string projectLocation;
        private string gameLocation;
        private string mpGameLocation;
        private string player1Name;
        private string player2Name;
        private bool alpakitCopyMod;
        private bool alpakitCloseGame;

        public MainViewModel(IPersistAndRestoreService persistAndRestoreService, INavigationService navigationService, IPluginService pluginService)
        {
            _persistAndRestoreService = persistAndRestoreService;
            _navigationService = navigationService;
            _pluginService = pluginService;
            pluginComboBoxEnabled = true;
            PluginSelectorViewModel = new PluginSelectionViewModel(persistAndRestoreService);
        }

        public void OnNavigatedTo(object parameter)
        {
            projectLocation = _persistAndRestoreService.Settings.ProjectPath;
            engineLocation = _persistAndRestoreService.Settings.UnrealEnginePath;
            gameLocation = _persistAndRestoreService.Settings.SatisfactoryPath;
            player1Name = _persistAndRestoreService.Settings.Player1Name;
            player2Name = _persistAndRestoreService.Settings.Player2Name;
            args1 = _persistAndRestoreService.Settings.Player1Args;
            args2 = _persistAndRestoreService.Settings.Player2Args;
            mpGameLocation = _persistAndRestoreService.Settings.Player2SatisfactoryPath;
            SelectedPlugin = _persistAndRestoreService.Settings.CurrentPlugin;
            alpakitCopyMod = _persistAndRestoreService.Settings.AlpakitCopyModToGame;
            alpakitCloseGame = _persistAndRestoreService.Settings.AlpakitCloseGame;
            if (string.IsNullOrEmpty(projectLocation))
            {
                //Navigate to Settings
                _navigationService.NavigateTo(typeof(SettingsViewModel).FullName);
            }
            else
            {
                SelectedPlugin = _pluginService.SelectedPlugin;
            }
        }

        public void OnNavigatedFrom()
        {

        }

        private void PopulatePluginList()
        {
            var pluginDirectory = projectLocation + "//Plugins";
            if (Directory.Exists(pluginDirectory))
            {
                List<string> pluginDirs = new();
                foreach (var directory in Directory.GetDirectories(pluginDirectory))
                {
                    var di = new DirectoryInfo(directory);
                    pluginDirs.Add(di.Name);
                }
                PluginList = pluginDirs;
            }
        }

        private AsyncRelayCommand generateVSFiles;
        public ICommand GenerateVSFiles => generateVSFiles ??= new AsyncRelayCommand(PerformGenerateVSFiles);

        private async Task PerformGenerateVSFiles()
        {
            OutputText = "Generating Visual Studio Files..." + Environment.NewLine;
            var exitCode = await RunProcess(@$"{engineLocation}\Binaries\DotNET\UnrealBuildTool.exe", @$"-projectfiles -project=""{projectLocation}\FactoryGame.uproject"" -game -rocket -progress");
            SendProcessFinishedMessage(exitCode, "Visual Studio File Generation");
        }

        private Task<int> RunProcess(string fileName, string arguments = "", bool redirectOutput = true)
        {
            var tcs = new TaskCompletionSource<int>();
            System.Diagnostics.Process process = new();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = redirectOutput
            };
            process.Exited += (sender, args) =>
                {
                    tcs.SetResult(process.ExitCode);
                    process.Dispose();
                };
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += Cmd_DataReceived;
            process.Start();
            if (redirectOutput)
            {
                process.BeginOutputReadLine();
            }

            return tcs.Task;
        }

        private void SendProcessFinishedMessage(int exitCode, string prefix)
        {
            OutputText += exitCode switch
            {
                0 => $"{prefix} Successful",
                2 => $"{prefix} Failed: Unable to find a needed file. Double check your directory paths",
                3 => $"{prefix} Failed: Unable to find a needed path. Double check your directory paths",
                5 => $"{prefix} Failed: Access Denied to something",
                _ => $"{prefix} Failed",
            };
        }

        private void Cmd_DataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputText += e.Data + Environment.NewLine;
        }

        internal string GetFromResources(string resourceName)
        {
            Assembly assem = this.GetType().Assembly;
            using (Stream stream = assem.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private AsyncRelayCommand buildForDevelopmentEditor;
        public ICommand BuildForDevelopmentEditor => buildForDevelopmentEditor ??= new AsyncRelayCommand(PerformBuildForDevelopmentEditor);

        private async Task<int> RunBuild(bool isShipping)
        {
            //"C:\Program Files\Unreal Engine - CSS\Engine\Build\BatchFiles\Build.bat" FactoryGame Win64 Shipping -Project="$(SolutionDir)FactoryGame.uproject" -WaitMutex -FromMsBuild
            //"C:\Program Files\Unreal Engine - CSS\Engine\Build\BatchFiles\Build.bat" FactoryGameEditor Win64 Development -Project="$(SolutionDir)FactoryGame.uproject" -WaitMutex -FromMsBuild

            string environmentToBuild = isShipping ? "FactoryGame Win64 Shipping" : "FactoryGameEditor Win64 Development";
            return await RunProcess(@$"`{engineLocation}\Build\BatchFiles\Build.bat`".SetQuotes(), @$"{environmentToBuild} -Project=""{projectLocation}\FactoryGame.uproject"" -WaitMutex -FromMsBuild");

        }

        private async Task PerformBuildForDevelopmentEditor()
        {
            OutputText = "Building Development Editor..." + Environment.NewLine;
            var exitCode = await RunBuild(false);
            SendProcessFinishedMessage(exitCode, "Build for Development Editor");
        }

        private AsyncRelayCommand buildForShipping;
        public ICommand BuildForShipping => buildForShipping ??= new AsyncRelayCommand(PerformBuildForShipping);

        private async Task PerformBuildForShipping()
        {
            OutputText = "Building Shipping..." + Environment.NewLine;
            var exitCode = await RunBuild(true);
            SendProcessFinishedMessage(exitCode, "Build for Shipping");
        }

        private AsyncRelayCommand runAlpakit;
        public ICommand RunAlpakit => runAlpakit ??= new AsyncRelayCommand(PerformRunAlpakit);

        private async Task Alpakit(bool launchGame)
        {
            if (alpakitCloseGame)
            {
                CloseRunningSatisfactoryProcesses();
            }

            OutputText = "Running Alpakit..." + Environment.NewLine;
            string alpakitArgs = alpakitCopyMod ? @$" -CopyToGameDir -GameDir=`{gameLocation}`" : "";
            int exitCode = await RunProcess(@$"{engineLocation}\Build\BatchFiles\RunUAT.bat", $@" -ScriptsForProject=`{projectLocation}\FactoryGame.uproject` PackagePlugin -Project=`{projectLocation}\FactoryGame.uproject` -PluginName=`{SelectedPlugin}` {alpakitArgs}".SetQuotes());
            SendProcessFinishedMessage(exitCode, "Alpakit");

            if (exitCode == 0 && launchGame)
            {
                OutputText += Environment.NewLine + "Launching Satisfactory...";
                PerformLaunchSatisfactory();
            }

        }

        private void CloseRunningSatisfactoryProcesses()
        {
            Process[] processlist = Process.GetProcessesByName("Satisfactory.exe");
            if (processlist.Length > 0)
            {
                OutputText = "Stopping existing Satisfactory processess..." + Environment.NewLine;
                foreach (var process in processlist)
                {
                    process.Kill();
                }
            }
        }

        private async Task PerformRunAlpakit()
        {
            Alpakit(false);
        }

        private AsyncRelayCommand runAlpakitAndLaunch;
        public ICommand RunAlpakitAndLaunch => runAlpakitAndLaunch ??= new AsyncRelayCommand(PerformRunAlpakitAndLaunch);

        private async Task PerformRunAlpakitAndLaunch()
        {
            Alpakit(true);
        }

        private async Task PerformLaunchSatisfactory()
        {
            OutputText = "Launching Satisfactory...";
            RunProcess(@$"{gameLocation}\FactoryGame.exe", "", false);
        }

        private AsyncRelayCommand runAllChecked;
        public ICommand RunAllChecked => runAllChecked ??= new AsyncRelayCommand(PerformRunAllChecked);

        private async Task PerformRunAllChecked()
        {
            if (AIOGenerateVSFiles)
            {
                await PerformGenerateVSFiles();
            }
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
                await PerformRunAlpakit();
            }
            if (AIOLaunchGame)
            {
                await PerformLaunchSatisfactory();
            }
            if (AIOLaunchMP)
            {
                await PerformLaunchMPTesting();
            }
        }

        private System.Collections.IEnumerable pluginList;

        public System.Collections.IEnumerable PluginList { get => pluginList; set => SetProperty(ref pluginList, value); }

        private bool aIOGenerateVSFiles;
        public bool AIOGenerateVSFiles { get => aIOGenerateVSFiles; set => SetProperty(ref aIOGenerateVSFiles, value); }

        private bool aIOBuildDevEditor;
        public bool AIOBuildDevEditor { get => aIOBuildDevEditor; set => SetProperty(ref aIOBuildDevEditor, value); }

        private bool aIOBuildShipping;
        public bool AIOBuildShipping { get => aIOBuildShipping; set => SetProperty(ref aIOBuildShipping, value); }

        private bool aIOAlpakit;
        public bool AIOAlpakit { get => aIOAlpakit; set => SetProperty(ref aIOAlpakit, value); }

        private bool aIOLaunchGame;
        public bool AIOLaunchGame { get => aIOLaunchGame; set => SetProperty(ref aIOLaunchGame, value); }

        private bool aIOLaunchMP;
        public bool AIOLaunchMP { get => aIOLaunchMP; set => SetProperty(ref aIOLaunchMP, value); }

        private object selectedPlugin;

        public object SelectedPlugin
        {
            get => selectedPlugin;
            set
            {
                SetProperty(ref selectedPlugin, value);
                _persistAndRestoreService.Settings.CurrentPlugin = value.ToString();
                _persistAndRestoreService.PersistData();
            }
        }

        private RelayCommand createCPPFiles;
        public ICommand CreateCPPFiles => createCPPFiles ??= new RelayCommand(PerformCreateCPPFiles);

        private void PerformCreateCPPFiles()
        {
            //Make Directory Structure
            var pluginDirectoryLocation = $"{projectLocation}//Plugins//{SelectedPlugin}";
            var sourceDir = Directory.CreateDirectory($"{pluginDirectoryLocation}//Source//{SelectedPlugin}");
            var publicDir = Directory.CreateDirectory($"{pluginDirectoryLocation}//Source//{SelectedPlugin}//Public");
            var privateDir = Directory.CreateDirectory($"{pluginDirectoryLocation}//Source//{SelectedPlugin}//Private");

            //Make Files
            var buildCS = GetFromResources("SatisfactoryModdingHelper.Templates.Buildcs.txt").Replace("[PluginReference]", SelectedPlugin.ToString());
            var moduleH = GetFromResources("SatisfactoryModdingHelper.Templates.Module.h.txt").Replace("[PluginReference]", SelectedPlugin.ToString());
            var moduleCPP = GetFromResources("SatisfactoryModdingHelper.Templates.Module.cpp.txt").Replace("[PluginReference]", SelectedPlugin.ToString());

            FileService.WriteAllTextIfNew($"{sourceDir}//{SelectedPlugin}.Build.cs", buildCS);
            FileService.WriteAllTextIfNew($"{publicDir}//{SelectedPlugin}Module.h", moduleH);
            FileService.WriteAllTextIfNew($"{privateDir}//{SelectedPlugin}Module.cpp", moduleCPP);

            OutputText = "Base C++ Directories and Files Created";

            //Edit uplugin
            string upluginText = File.ReadAllText($"{pluginDirectoryLocation}//{SelectedPlugin}.uplugin");
            UPluginModel uPlugin = JsonConvert.DeserializeObject<UPluginModel>(upluginText);

            if (uPlugin.Modules == null || uPlugin.Modules.Count == 0)
            {
                Models.ModuleModel module = new();
                module.Name = SelectedPlugin.ToString();
                module.Type = "Runtime";
                module.LoadingPhase = "Default";
                if (uPlugin.Modules == null)
                {
                    uPlugin.Modules = new ObservableCollection<Models.ModuleModel>();
                }
                uPlugin.Modules.Add(module);
                upluginText = JsonConvert.SerializeObject(uPlugin, Formatting.Indented);
                File.WriteAllText($"{pluginDirectoryLocation}//{SelectedPlugin}.uplugin", upluginText);
            }
        }

        private string outputText;

        public string OutputText { get => outputText; set => SetProperty(ref outputText, value); }

        private AsyncRelayCommand launchMPTesting;
        public ICommand LaunchMPTesting => launchMPTesting ??= new AsyncRelayCommand(PerformLaunchMPTesting);

        private string args1;
        private string args2;

        private async Task PerformLaunchMPTesting()
        {
            //Build launch strings
            string launchStringArgs1 = $"-EpicPortal -NoSteamClient -Username=`{player1Name}` {args1}".SetQuotes();
            string launchStringArgs2 = $"-EpicPortal -NoSteamClient -Username=`{player2Name}` {args2}".SetQuotes();
            RunProcess(@$"`{gameLocation}\FactoryGame.exe`".SetQuotes(), launchStringArgs1, false);
            Thread.Sleep(1000);
            if (mpGameLocation.Length > 0)
            {
                await MirrorInstallForMPTest();
                RunProcess(@$"`{mpGameLocation}\FactoryGame.exe`".SetQuotes(), launchStringArgs2, false);
            }
            else
            {
                RunProcess(@$"`{gameLocation}\FactoryGame.exe`".SetQuotes(), launchStringArgs2, false);
            }
        }

        private RelayCommand launchMPTestingHost;
        public ICommand LaunchMPTestingHost => launchMPTestingHost ??= new RelayCommand(PerformLaunchMPTestingHost);

        private void PerformLaunchMPTestingHost()
        {
            LaunchMP(player1Name, args1);
        }

        private RelayCommand launchMPTestingClient;
        public ICommand LaunchMPTestingClient => launchMPTestingClient ??= new RelayCommand(PerformLaunchMPTestingClient);

        private void PerformLaunchMPTestingClient()
        {
            LaunchMP(player2Name, args2);
        }

        private void LaunchMP(string playerName, string args)
        {
            string launchStringArgs = @$"-EpicPortal -NoSteamClient -Username=`{playerName}` {args}";
            RunProcess(@$"`{gameLocation}\FactoryGame.exe`".SetQuotes(), launchStringArgs.SetQuotes(), false);
        }

        public void OnStartingNavigateFrom()
        {
        }

        private bool pluginComboBoxEnabled;

        public bool PluginComboBoxEnabled { get => pluginComboBoxEnabled; set => SetProperty(ref pluginComboBoxEnabled, value); }

        private object pluginSelectorViewModel;

        public object PluginSelectorViewModel { get => pluginSelectorViewModel; set => SetProperty(ref pluginSelectorViewModel, value); }

        private async Task MirrorInstallForMPTest()
        {
            string launchStringArgs = @$"`{gameLocation}` `{mpGameLocation}` /PURGE /MIR /XD Configs /R:2 /W:2 /NS /NDL /NFL /NP";
            await RunProcess(@$"`ROBOCOPY.EXE`".SetQuotes(), launchStringArgs.SetQuotes(), false);
        }
    }
}