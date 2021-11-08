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
//using Alpakit.Automation;

namespace SatisfactoryModdingHelper.ViewModels
{
    public class MainViewModel : ObservableObject, INavigationAware
    {
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private string pluginName;
        private string engineLocation;
        private string projectLocation;
        private string gameLocation;
        private bool alpakitCopyMod;
        private bool alpakitCloseGame;

        public MainViewModel(IPersistAndRestoreService persistAndRestoreService)
        {
            _persistAndRestoreService = persistAndRestoreService;
        }

        public void OnNavigatedTo(object parameter)
        {
            projectLocation = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_Project);
            engineLocation = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_UE);
            gameLocation = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Locations_Satisfactory);
            alpakitCopyMod = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Alpakit_CopyMod);
            alpakitCloseGame = _persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Alpakit_CloseGame);
            PopulatePluginList();
        }

        public void OnNavigatedFrom()
        {
            _persistAndRestoreService.PersistData();
        }

        private void PopulatePluginList()
        {
            var pluginDirectory = projectLocation + "//Plugins";
            List<string> pluginDirs = new List<string>();
            foreach (var directory in Directory.GetDirectories(pluginDirectory))
            {
                var di = new DirectoryInfo(directory);
                pluginDirs.Add(di.Name);
            }
            PluginList = pluginDirs;
        }

        private RelayCommand generateVSFiles;
        public ICommand GenerateVSFiles => generateVSFiles ??= new RelayCommand(PerformGenerateVSFiles);

        private async void PerformGenerateVSFiles()
        {
            OutputText = "Generating Visual Studio Files..." + Environment.NewLine;
            await Task.Run(() =>
            {
                RunProcess(@$"{engineLocation}\Binaries\DotNET\UnrealBuildTool.exe", @$"-projectfiles -project=""{projectLocation}\FactoryGame.uproject"" -game -rocket -progress");
            });

            OutputText += "Visual Studio File Generation Complete";
        }

        private void RunProcess(string fileName, string arguments = "")
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            process.OutputDataReceived += cmd_DataReceived;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
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

        private RelayCommand buildForDevelopmentEditor;
        public ICommand BuildForDevelopmentEditor => buildForDevelopmentEditor ??= new RelayCommand(PerformBuildForDevelopmentEditor);

        private async void PerformBuildForDevelopmentEditor()
        {
            //"C:\Program Files\Unreal Engine - CSS\Engine\Build\BatchFiles\Build.bat" FactoryGameEditor Win64 Development -Project="$(SolutionDir)FactoryGame.uproject" -WaitMutex -FromMsBuild
            OutputText = "Building Development Editor..." + Environment.NewLine;
            await Task.Run(() =>
            {
                RunProcess(@$"{engineLocation}\Build\BatchFiles\Build.bat", @$"FactoryGameEditor Win64 Development -Project=""{projectLocation}\FactoryGame.uproject"" -WaitMutex -FromMsBuild");
            });

            OutputText += "Build for Development Editor Complete";

            //Dev
            //"C:\Program Files\Unreal Engine - CSS\Engine\Build\BatchFiles\Build.bat" FactoryGame Win64 Development -Project="$(SolutionDir)FactoryGame.uproject" -WaitMutex -FromMsBuild
        }

        private RelayCommand buildForShipping;
        public ICommand BuildForShipping => buildForShipping ??= new RelayCommand(PerformBuildForShipping);

        private async void PerformBuildForShipping()
        {
            //"C:\Program Files\Unreal Engine - CSS\Engine\Build\BatchFiles\Build.bat" FactoryGame Win64 Shipping -Project="$(SolutionDir)FactoryGame.uproject" -WaitMutex -FromMsBuild
            OutputText = "Building Shipping..." + Environment.NewLine;
            await Task.Run(() =>
            {
                RunProcess(@$"{engineLocation}\Build\BatchFiles\Build.bat", @$"FactoryGame Win64 Shipping -Project=""{projectLocation}\FactoryGame.uproject"" -WaitMutex -FromMsBuild");
            });

            OutputText += "Build for Shipping Complete";
        }

        private RelayCommand runAlpakit;
        public ICommand RunAlpakit => runAlpakit ??= new RelayCommand(PerformRunAlpakit);

        private async void PerformRunAlpakit()
        {
            //
            //Get Engine path\Engine\Build\BatchFiles\RunUAT.bat

            OutputText = "Running Alpakit..." + Environment.NewLine;
            await Task.Run(() =>
            {
                if (alpakitCopyMod)
                {
                    RunProcess(@$"{engineLocation}\Build\BatchFiles\RunUAT.bat", $@"-ScriptsForProject = ""{projectLocation}\FactoryGame.uproject"" PackagePlugin -Project = ""{projectLocation}\FactoryGame.uproject"" -PluginName = ""{pluginName}"" -CopyToGameDir -GameDir = ""{gameLocation}""");
                }
                else
                {
                    RunProcess(@$"{engineLocation}\Build\BatchFiles\RunUAT.bat", $@"-ScriptsForProject = ""{projectLocation}\FactoryGame.uproject"" PackagePlugin -Project = ""{projectLocation}\FactoryGame.uproject"" -PluginName = ""{pluginName}""");
                }
            });

            OutputText += "Alpakit Complete";

            //Alpakit.Automation.FactoryGameParams factoryGameParams = new FactoryGameParams();
            //factoryGameParams.CopyToGameDirectory = (bool)_persistAndRestoreService.GetSavedProperty(Properties.Resources.Settings_Alpakit_CopyMod_Value);
        }

        private RelayCommand runAlpakitAndLaunch;
        public ICommand RunAlpakitAndLaunch => runAlpakitAndLaunch ??= new RelayCommand(PerformRunAlpakitAndLaunch);

        private async void PerformRunAlpakitAndLaunch()
        {
            OutputText = "Running Alpakit..." + Environment.NewLine;
            await Task.Run(() =>
            {
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
                RunProcess(@$"{engineLocation}\Build\BatchFiles\RunUAT.bat", $@"-ScriptsForProject = ""{projectLocation}\FactoryGame.uproject"" PackagePlugin -Project = ""{projectLocation}\FactoryGame.uproject"" -PluginName = ""{pluginName}""");
            });

            OutputText += "Alpakit Complete";
            OutputText += Environment.NewLine + "Launching Satisfactory...";
        }

        private async void PerformLaunchSatisfactory()
        {
            OutputText = "Launching Satisfactory...";
            await Task.Run(() =>
            {
                RunProcess(@$"{gameLocation}\satisfactory.exe");
            });
        }

        private RelayCommand runAllChecked;
        public ICommand RunAllChecked => runAllChecked ??= new RelayCommand(PerformRunAllChecked);

        private void PerformRunAllChecked()
        {
            if (AIOGenerateVSFiles)
            {
                PerformGenerateVSFiles();
            }
            if (AIOBuildDevEditor)
            {
                PerformBuildForDevelopmentEditor();
            }
            if (AIOBuildShipping)
            {
                PerformBuildForShipping();
            }
            if (AIOAlpakit)
            {
                PerformRunAlpakit();
            }
            if (AIOLaunchGame)
            {
                PerformLaunchSatisfactory();
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
    }
}
