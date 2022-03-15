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
//using Alpakit.Automation;

namespace SatisfactoryModdingHelper.ViewModels
{
    public class MainViewModel : ObservableObject, INavigationAware
    {
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private readonly INavigationService _navigationService;
        private string pluginName;
        private string engineLocation;
        private string projectLocation;
        private string gameLocation;
        private string player1Name;
        private string player2Name;
        private bool alpakitCopyMod;
        private bool alpakitCloseGame;
        private int runningProcessID;
        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;


        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);


        public MainViewModel(IPersistAndRestoreService persistAndRestoreService, INavigationService navigationService)
        {
            _persistAndRestoreService = persistAndRestoreService;
            _navigationService = navigationService;
            pluginComboBoxEnabled = true;
        }

        public void OnNavigatedTo(object parameter)
        {
            projectLocation = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_Project);
            engineLocation = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_UE);
            gameLocation = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_Satisfactory);
            player1Name = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_MP_Player1Name);
            player2Name = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_MP_Player2Name);
            args1 = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_MP_Args1);
            args2 = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_MP_Args2);

            alpakitCopyMod = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Alpakit_CopyMod) == null ? false : _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Alpakit_CopyMod);
            alpakitCloseGame = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Alpakit_CloseGame) == null ? false : _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Alpakit_CloseGame);
            if (string.IsNullOrEmpty(projectLocation))
            {
                //Navigate to Settings
                _navigationService.NavigateTo(typeof(SettingsViewModel).FullName);
            }
            else
            {
                PopulatePluginList();
            }
        }

        public void OnNavigatedFrom()
        {
            _persistAndRestoreService.PersistData();
        }

        private void PopulatePluginList()
        {
            var pluginDirectory = projectLocation + "//Plugins";
            if (Directory.Exists(pluginDirectory))
            {
                List<string> pluginDirs = new List<string>();
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

        private Task<int> RunProcess(string fileName, string arguments = "", bool redirectOutput = true, int posX = -1, int posY = -1)
        {
            var tcs = new TaskCompletionSource<int>();
            System.Diagnostics.Process process = new System.Diagnostics.Process();
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
                runningProcessID = 0;
            };
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += cmd_DataReceived;
            process.Start();
            runningProcessID = process.Id;
            if (redirectOutput)
            {
                process.BeginOutputReadLine();
            }
            if (posX >= 0 || posY >= 0)
            {
                IntPtr handle = process.MainWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    MoveWindow(handle, posX, posY, 0, 0, false);
                }
            }
            //process.WaitForExit();
            return tcs.Task;
        }

        private void SendProcessFinishedMessage(int exitCode, string prefix)
        {
            switch (exitCode)
            {
                case 0:
                    OutputText += $"{prefix} Successful";
                    break;
                case 2:
                    OutputText += $"{prefix} Failed: Unable to find a needed file. Double check your directory paths";
                    break;
                case 3:
                    OutputText += $"{prefix} Failed: Unable to find a needed path. Double check your directory paths";
                    break;
                case 5:
                    OutputText += $"{prefix} Failed: Access Denied to something";
                    break;
                default:
                    OutputText += $"{prefix} Failed";
                    break;
            }
        }

        private void cmd_DataReceived(object sender, DataReceivedEventArgs e)
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

        private async Task PerformBuildForDevelopmentEditor()
        {
            //"C:\Program Files\Unreal Engine - CSS\Engine\Build\BatchFiles\Build.bat" FactoryGameEditor Win64 Development -Project="$(SolutionDir)FactoryGame.uproject" -WaitMutex -FromMsBuild
            OutputText = "Building Development Editor..." + Environment.NewLine;
            var exitCode = await RunProcess(@$"{engineLocation}\Build\BatchFiles\Build.bat", @$"FactoryGameEditor Win64 Development -Project=""{projectLocation}\FactoryGame.uproject"" -WaitMutex -FromMsBuild");

            SendProcessFinishedMessage(exitCode, "Build for Development Editor");

            //Dev
            //"C:\Program Files\Unreal Engine - CSS\Engine\Build\BatchFiles\Build.bat" FactoryGame Win64 Development -Project="$(SolutionDir)FactoryGame.uproject" -WaitMutex -FromMsBuild
        }

        private AsyncRelayCommand buildForShipping;
        public ICommand BuildForShipping => buildForShipping ??= new AsyncRelayCommand(PerformBuildForShipping);

        private async Task PerformBuildForShipping()
        {
            //"C:\Program Files\Unreal Engine - CSS\Engine\Build\BatchFiles\Build.bat" FactoryGame Win64 Shipping -Project="$(SolutionDir)FactoryGame.uproject" -WaitMutex -FromMsBuild
            OutputText = "Building Shipping..." + Environment.NewLine;
            var exitCode = await RunProcess(@$"{engineLocation}\Build\BatchFiles\Build.bat", @$"FactoryGame Win64 Shipping -Project=""{projectLocation}\FactoryGame.uproject"" -WaitMutex -FromMsBuild");

            SendProcessFinishedMessage(exitCode, "Build for Shipping");
            //OutputText += "Build for Shipping Complete";
        }

        private AsyncRelayCommand runAlpakit;
        public ICommand RunAlpakit => runAlpakit ??= new AsyncRelayCommand(PerformRunAlpakit);

        private async Task PerformRunAlpakit()
        {
            //
            //Get Engine path\Engine\Build\BatchFiles\RunUAT.bat

            OutputText = "Running Alpakit..." + Environment.NewLine;
            int exitCode;
            if (alpakitCopyMod)
            {
                exitCode = await RunProcess(@$"{engineLocation}\Build\BatchFiles\RunUAT.bat", $@" -ScriptsForProject=""{projectLocation}\FactoryGame.uproject"" PackagePlugin -Project=""{projectLocation}\FactoryGame.uproject"" -PluginName=""{SelectedPlugin.ToString()}"" -CopyToGameDir -GameDir=""{gameLocation}""");
            }
            else
            {
                exitCode = await RunProcess(@$"{engineLocation}\Build\BatchFiles\RunUAT.bat", $@" -ScriptsForProject=""{projectLocation}\FactoryGame.uproject"" PackagePlugin -Project=""{projectLocation}\FactoryGame.uproject"" -PluginName=""{SelectedPlugin.ToString()}""");
            }

            SendProcessFinishedMessage(exitCode, "Alpakit");
            //OutputText += "Alpakit Complete";

            //Alpakit.Automation.FactoryGameParams factoryGameParams = new FactoryGameParams();
            //factoryGameParams.CopyToGameDirectory = (bool)_persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Alpakit_CopyMod_Value);
        }

        private AsyncRelayCommand runAlpakitAndLaunch;
        public ICommand RunAlpakitAndLaunch => runAlpakitAndLaunch ??= new AsyncRelayCommand(PerformRunAlpakitAndLaunch);

        private async Task PerformRunAlpakitAndLaunch()
        {
            OutputText = "Running Alpakit..." + Environment.NewLine;
            if (alpakitCloseGame)
            {
                //Check for running Satisfactory process
                Process[] processlist = Process.GetProcessesByName("Satisfactory.exe");
                if (processlist.Length > 0)
                {
                    foreach (var process in processlist)
                    {
                        process.Kill();
                    }
                }
            }
            var exitCode = await RunProcess(@$"{engineLocation}\Build\BatchFiles\RunUAT.bat", $@" -ScriptsForProject=""{projectLocation}\FactoryGame.uproject"" PackagePlugin -Project=""{projectLocation}\FactoryGame.uproject"" -PluginName=""{SelectedPlugin.ToString()}"" -CopyToGameDir -GameDir=""{gameLocation}""");

            SendProcessFinishedMessage(exitCode, "Alpakit");
            //OutputText += "Alpakit Complete";
            if (exitCode == 0)
            {
                OutputText += Environment.NewLine + "Launching Satisfactory...";
                PerformLaunchSatisfactory();
            }
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

        public object SelectedPlugin { get => selectedPlugin; set => SetProperty(ref selectedPlugin, value); }

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

            if (!File.Exists($"{sourceDir}//{SelectedPlugin}.Build.cs"))
            {
                File.WriteAllText($"{sourceDir}//{SelectedPlugin}.Build.cs", buildCS);
            }

            if (!File.Exists($"{publicDir}//{SelectedPlugin}Module.h"))
            {
                File.WriteAllText($"{publicDir}//{SelectedPlugin}Module.h", moduleH);
            }

            if (!File.Exists($"{privateDir}//{SelectedPlugin}Module.cpp"))
            {
                File.WriteAllText($"{privateDir}//{SelectedPlugin}Module.cpp", moduleCPP);
            }

            OutputText = "Base C++ Directories and Files Created";

            //Edit uplugin
            string upluginText = File.ReadAllText($"{pluginDirectoryLocation}//{SelectedPlugin}.uplugin");
            UPluginModel uPlugin = JsonConvert.DeserializeObject<UPluginModel>(upluginText);

            if (uPlugin.Modules == null || uPlugin.Modules.Count == 0)
            {
                Models.Module module = new Models.Module();
                module.Name = SelectedPlugin.ToString();
                module.Type = "Runtime";
                module.LoadingPhase = "Default";
                if (uPlugin.Modules == null)
                {
                    uPlugin.Modules = new List<Models.Module>();
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
            string launchStringArgs1 = $"-EpicPortal -NoSteamClient -Username=\"{player1Name}\" {args1}";
            string launchStringArgs2 = $"-EpicPortal -NoSteamClient -Username=\"{player2Name}\" {args2}";
            RunProcess($"\"{gameLocation}\\FactoryGame.exe\"", launchStringArgs1, false);
            Thread.Sleep(1000);
            RunProcess($"\"{gameLocation}\\FactoryGame.exe\"", launchStringArgs2, false);
        }

        private RelayCommand launchMPTestingHost;
        public ICommand LaunchMPTestingHost => launchMPTestingHost ??= new RelayCommand(PerformLaunchMPTestingHost);

        private void PerformLaunchMPTestingHost()
        {
            string launchStringArgs1 = $"-EpicPortal -NoSteamClient -Username=\"{player1Name}\" {args1}";
            RunProcess($"\"{gameLocation}\\FactoryGame.exe\"", launchStringArgs1, false);
        }

        private RelayCommand launchMPTestingClient;
        public ICommand LaunchMPTestingClient => launchMPTestingClient ??= new RelayCommand(PerformLaunchMPTestingClient);

        private void PerformLaunchMPTestingClient()
        {
            string launchStringArgs2 = $"-EpicPortal -NoSteamClient -Username=\"{player2Name}\" {args2}";
            RunProcess($"\"{gameLocation}\\FactoryGame.exe\"", launchStringArgs2, false);
        }

        private bool pluginComboBoxEnabled;

        public bool PluginComboBoxEnabled { get => pluginComboBoxEnabled; set => SetProperty(ref pluginComboBoxEnabled, value); }
    }
}
