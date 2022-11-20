using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppNotifications;
using Newtonsoft.Json;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Core.Contracts.Services;
using SatisfactoryModdingHelper.Core.Services;
using SatisfactoryModdingHelper.Dialogs;
using SatisfactoryModdingHelper.Extensions;
using SatisfactoryModdingHelper.Models;
using SatisfactoryModdingHelper.Services;
using Windows.Storage;

namespace SatisfactoryModdingHelper.ViewModels;

public class CPPViewModel : ObservableRecipient, INavigationAware
{
    private readonly ILocalSettingsService _settingsService;
    private readonly IPluginService _pluginService;
    private readonly IFileService _fileService;
    private readonly IAppNotificationService _appNotificationService;
    private readonly IProcessService _processService;
    private string projectLocation;
    private string gameLocation;
    private string sourceDir;
    private string publicDir;
    private string privateDir;
    private string engineLocation = "";

    public CPPViewModel(IPluginService pluginService, IFileService fileService, ILocalSettingsService settingsService, IAppNotificationService appNotificationService, IProcessService processService)
    {
        _pluginService = pluginService;
        _fileService = fileService;
        _settingsService = settingsService;
        _appNotificationService=appNotificationService;
        _processService = processService;
    }

    private object selectedPlugin;
    public object SelectedPlugin
    {
        get => selectedPlugin;
        set
        {
            if (SetProperty(ref selectedPlugin, value))
            {

            }
        }
    }

    public void OnNavigatedTo(object parameter)
    {
        projectLocation = _settingsService.Settings.UProjectFolderPath;
        SelectedPlugin = _pluginService.SelectedPlugin;
        engineLocation = _settingsService.Settings.UnrealEngineFolderPath;
        gameLocation = _settingsService.Settings.SatisfactoryFolderPath;


    }
    public void OnNavigatedFrom()
    {
        // Method intentionally left empty.
    }

    private bool buttonsEnabled;
    public bool ButtonEnabled
    {
        get => buttonsEnabled;
        set => SetProperty(ref buttonsEnabled, value);
    }

    internal async Task<string> GetFromResources(string resourceName)
    {
        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Templates/{resourceName}"));
        using (Stream stream = await file.OpenStreamForReadAsync())
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
    internal async Task<string> GetFromResources(string resourceName, string oldValue, string newValue)
    {
        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Templates/{resourceName}"));
        string fileText;
        using (Stream stream = await file.OpenStreamForReadAsync())
        {
            using (var reader = new StreamReader(stream))
            {
                fileText = reader.ReadToEnd();
            }
        }
        return fileText.Replace(oldValue, newValue);
    }


    private AsyncRelayCommand generateVSFiles;
    public ICommand GenerateVSFiles => generateVSFiles ??= new AsyncRelayCommand(PerformGenerateVSFiles);

    private async Task PerformGenerateVSFiles()
    {
        var result = await _processService.RunProcess(@$"{engineLocation}\Engine\Binaries\DotNET\UnrealBuildTool.exe", @$"-projectfiles -project=""{projectLocation}\FactoryGame.uproject"" -game -rocket -progress");
        _appNotificationService.SendNotification($"VS Files have been generated");
    }

    private AsyncRelayCommand<string> generateModuleFiles;
    public ICommand GenerateModuleFiles => generateModuleFiles ??= new AsyncRelayCommand<string>(PerformGenerateModuleFiles);

    public async Task PerformGenerateModuleFiles(string showNotification = "True")
    {
        //Make Directory Structure
        var pluginDirectoryLocation = $"{projectLocation}//Plugins//{SelectedPlugin}";
        sourceDir = Directory.CreateDirectory($"{pluginDirectoryLocation}//Source//{SelectedPlugin}").FullName;
        publicDir = Directory.CreateDirectory($"{pluginDirectoryLocation}//Source//{SelectedPlugin}//Public").FullName;
        privateDir = Directory.CreateDirectory($"{pluginDirectoryLocation}//Source//{SelectedPlugin}//Private").FullName;

        //Make Files
        var buildCS = await GetFromResources("Buildcs.txt", "[PluginReference]", SelectedPlugin.ToString());
        var moduleH = await GetFromResources("Module.h.txt", "[PluginReference]", SelectedPlugin.ToString());
        var moduleCPP = await GetFromResources("Module.cpp.txt", "[PluginReference]", SelectedPlugin.ToString());

        _fileService.WriteAllTextIfNew($"{sourceDir}//{SelectedPlugin}.Build.cs", buildCS);
        _fileService.WriteAllTextIfNew($"{publicDir}//{SelectedPlugin}Module.h", moduleH);
        _fileService.WriteAllTextIfNew($"{privateDir}//{SelectedPlugin}Module.cpp", moduleCPP);

        //OutputText = "Base C++ Directories and Files Created";

        //Edit uplugin
        string upluginText = File.ReadAllText($"{pluginDirectoryLocation}//{SelectedPlugin}.uplugin");
        UPluginModel uPlugin = JsonConvert.DeserializeObject<UPluginModel>(upluginText);

        if (uPlugin.Modules == null || uPlugin.Modules.Count == 0)
        {
            Models.ModuleModel module = new();
            module.Name = SelectedPlugin.ToString();
            module.Type = "Runtime";
            module.LoadingPhase = "Default";
            uPlugin.Modules ??= new ObservableCollection<ModuleModel>();
            uPlugin.Modules.Add(module);
            upluginText = JsonConvert.SerializeObject(uPlugin, Formatting.Indented);
            File.WriteAllText($"{pluginDirectoryLocation}//{SelectedPlugin}.uplugin", upluginText);

        }

        if (showNotification == "True")
        {
            _appNotificationService.SendNotification($"CPP Module Files have been created and UPlugin has been updated");
        }
    }

    public async void PerformAddBPFL(string className)
    {

        await PerformGenerateModuleFiles("False"); //Should probably do some sort of checks

        var hFile = await GetFromResources("BPFL.h.txt", "[ClassName]", className);
        var cppFile = await GetFromResources("BPFL.cpp.txt", "[ClassName]", className);

        hFile = hFile.Replace("[PluginReferenceUC]", SelectedPlugin.ToString().ToUpper());

        var headerCreated = _fileService.WriteAllTextIfNew($"{publicDir}//{className}.h", hFile);
        var cppCreated = _fileService.WriteAllTextIfNew($"{privateDir}//{className}.cpp", cppFile);

        if (headerCreated && cppCreated)
        {
            _appNotificationService.SendNotification($"BPFL {className} has been created");
        }
        else
        {
            _appNotificationService.SendNotification($"Unable to create BPFL {className}. Class already exists");
        }
    }

    public async void PerformAddSubsystem(string className)
    {
        await PerformGenerateModuleFiles("False"); //Should probably do some sort of checks

        var hFile = await GetFromResources("Subsystem.h.txt", "[ClassName]", className);
        var cppFile = await GetFromResources("Subsystem.cpp.txt", "[ClassName]", className);

        hFile = hFile.Replace("[PluginReferenceUC]", SelectedPlugin.ToString().ToUpper());

        var headerCreated = _fileService.WriteAllTextIfNew($"{publicDir}//{className}.h", hFile);
        var cppCreated = _fileService.WriteAllTextIfNew($"{privateDir}//{className}.cpp", cppFile);

        if (headerCreated && cppCreated)
        {
            _appNotificationService.SendNotification($"Subsystem {className} has been created");
        }
        else
        {
            _appNotificationService.SendNotification($"Unable to create Subsystem {className}. Class already exists");
        }
    }

    public async void PerformAddRCO(string className)
    {
        await PerformGenerateModuleFiles("False"); //Should probably do some sort of checks

        var hFile = await GetFromResources("RCO.h.txt", "[ClassName]", className);
        var cppFile = await GetFromResources("RCO.cpp.txt", "[ClassName]", className);

        hFile = hFile.Replace("[PluginReferenceUC]", SelectedPlugin.ToString().ToUpper());

        var headerCreated = _fileService.WriteAllTextIfNew($"{publicDir}//{className}.h", hFile);
        var cppCreated = _fileService.WriteAllTextIfNew($"{privateDir}//{className}.cpp", cppFile);

        if (headerCreated && cppCreated)
        {
            _appNotificationService.SendNotification($"RCO {className} has been created");
        }
        else
        {
            _appNotificationService.SendNotification($"Unable to create RCO {className}. Class already exists");
        }
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

    private AsyncRelayCommand copyCPPFiles;
    public ICommand CopyCPPFiles => copyCPPFiles ??= new AsyncRelayCommand(PerformCopyCPPFiles);
    private async Task PerformCopyCPPFiles()
    {
        //F:\SatisfactoryModMaking\SML-master\Plugins\CounterLimiter\Binaries\Win64
        //FactoryGame-CounterLimiter-Win64-Shipping.dll
        //FactoryGame-CounterLimiter-Win64-Shipping.pdb
        //FactoryGame-Win64-Shipping.modules
        //F:\Games\SteamLibrary\steamapps\common\Satisfactory\FactoryGame\Mods\CounterLimiter\Binaries\Win64
        var pluginBinariesLocation = $"{projectLocation}//Plugins//{SelectedPlugin}//Binaries//Win64";

        var dllFileName = $"FactoryGame-{SelectedPlugin}-Win64-Shipping.dll";
        var pdbFileName = $"FactoryGame-{SelectedPlugin}-Win64-Shipping.pdb";
        var modulesFileName = "FactoryGame-Win64-Shipping.modules";

        var dllSource = Path.Combine(pluginBinariesLocation, dllFileName);
        var pdbSource = Path.Combine(pluginBinariesLocation, pdbFileName);
        var modulesSource = Path.Combine(pluginBinariesLocation, modulesFileName);

        var pluginGameLocation = $"{gameLocation}//FactoryGame//Mods//{SelectedPlugin}//Binaries//Win64";

        var dllDest = Path.Combine(pluginGameLocation, dllFileName);
        var pdbDest = Path.Combine(pluginGameLocation, pdbFileName);
        var modulesDest = Path.Combine(pluginGameLocation, modulesFileName);

        if (File.Exists(dllSource))
        {
            File.Copy(dllSource, dllDest, overwrite: true);
        }
        if (File.Exists(pdbSource))
        {
            File.Copy(pdbSource, pdbDest, overwrite: true);
        }
        if (File.Exists(modulesSource))
        {
            File.Copy(modulesSource, modulesDest, overwrite: true);
        }
    }
}
