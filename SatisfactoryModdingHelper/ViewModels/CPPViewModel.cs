using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Dispatching;
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
using SatisfactoryModdingHelper.Helpers;
using SatisfactoryModdingHelper.Models;
using SatisfactoryModdingHelper.Services;
using Windows.Storage;

namespace SatisfactoryModdingHelper.ViewModels;

public class CPPViewModel : ObservableRecipient, INavigationAware
{
    private readonly ILocalSettingsService _settingsService;
    private readonly IModService _modService;
    private readonly IFileService _fileService;
    private readonly IAppNotificationService _appNotificationService;
    public readonly IProcessService _processService;
    //private string projectLocation;
    //private string gameLocation;
    private string sourceDir;
    private string publicDir;
    private string privateDir;
  //  private string engineLocation = "";

    public CPPViewModel(IModService modService, IFileService fileService, ILocalSettingsService settingsService, IAppNotificationService appNotificationService, IProcessService processService)
    {
        _modService = modService;
        _fileService = fileService;
        _settingsService = settingsService;
        _appNotificationService=appNotificationService;
        _processService = processService;
    }

    private object selectedMod;
    public object SelectedMod
    {
        get => selectedMod;
        set
        {
            if (SetProperty(ref selectedMod, value))
            {

            }
        }
    }

    public void OnNavigatedTo(object parameter)
    {
        //projectLocation = _settingsService.Settings.UProjectFolderPath;
        SelectedMod = _modService.SelectedMod;
        // engineLocation = _settingsService.Settings.UnrealEngineFolderPath;
        // gameLocation = _settingsService.Settings.SatisfactoryFolderPath;
        copyDLLAfterBuildShipping = _settingsService.Settings.CopyDLLAfterBuildShipping;
        RunUpdateOutput = true;
        UpdateOutput();

    }
    public void OnNavigatedFrom()
    {
        RunUpdateOutput = false;
    }

    private bool RunUpdateOutput = false;

    private ObservableCollection<string> outputList;
    public ObservableCollection<string> OutputList
    {
        get => outputList;
        set => SetProperty(ref outputList, value);
    }

    private bool copyDLLAfterBuildShipping;
    public bool CopyDLLAfterBuildShipping
    {
        get => copyDLLAfterBuildShipping;
        set
        {
            if (SetProperty(ref copyDLLAfterBuildShipping, value))
            {
                _settingsService.Settings.CopyDLLAfterBuildShipping = value;
                _settingsService.PersistData();
            }
        }
    }

    private bool buttonsEnabled;
    public bool ButtonEnabled
    {
        get => buttonsEnabled;
        set => SetProperty(ref buttonsEnabled, value);
    }

    private AsyncRelayCommand generateVSFiles;
    public ICommand GenerateVSFiles => generateVSFiles ??= new AsyncRelayCommand(PerformGenerateVSFiles);

    private async Task PerformGenerateVSFiles()
    {
        await _processService.GenerateVSFiles(_settingsService.Settings.UnrealBuildToolFilePath, _settingsService.Settings.UProjectFilePath);
        _appNotificationService.SendNotification(StringHelper.GenVSFilesComplete);
    }

    private AsyncRelayCommand<string> generateModuleFiles;
    public ICommand GenerateModuleFiles => generateModuleFiles ??= new AsyncRelayCommand<string>(PerformGenerateModuleFiles);

    public async Task PerformGenerateModuleFiles(string showNotification = "True")
    {
        //Make Directory Structure
        var modDirectoryLocation = StringHelper.GetModFolderPath(_settingsService.Settings.UProjectFolderPath, SelectedMod.ToString());
        sourceDir = Directory.CreateDirectory(StringHelper.GetModSourceFolderPath(modDirectoryLocation, SelectedMod.ToString())).FullName;
        publicDir = Directory.CreateDirectory(StringHelper.GetModPublicFolderPath(modDirectoryLocation, SelectedMod.ToString())).FullName;
        privateDir = Directory.CreateDirectory(StringHelper.GetModPrivateFolderPath(modDirectoryLocation, SelectedMod.ToString())).FullName;

        //Make Files
        var buildCS = await ResourceHelpers.GetTemplateResourceAndReplace(StringHelper.BuildcsTemplateName, StringHelper.TemplateModReferencePlaceholder, SelectedMod.ToString());
        var moduleH = await ResourceHelpers.GetTemplateResourceAndReplace(StringHelper.ModulehTemplateName, StringHelper.TemplateModReferencePlaceholder, SelectedMod.ToString());
        var moduleCPP = await ResourceHelpers.GetTemplateResourceAndReplace(StringHelper.ModulecppTemplateName, StringHelper.TemplateModReferencePlaceholder, SelectedMod.ToString());
        
        _fileService.WriteAllTextIfNew(StringHelper.GetBuildcsFilePath(sourceDir, SelectedMod.ToString()), buildCS);
        _fileService.WriteAllTextIfNew(StringHelper.GetModulehFilePath(sourceDir, SelectedMod.ToString()), moduleH);
        _fileService.WriteAllTextIfNew(StringHelper.GetModulecppFilePath(sourceDir, SelectedMod.ToString()), moduleCPP);

        //OutputText = "Base C++ Directories and Files Created";

        //Edit uplugin
        var upluginPath = StringHelper.GetUpluginFilePath(modDirectoryLocation, SelectedMod.ToString());
        var upluginText = File.ReadAllText(upluginPath);
        var uPlugin = JsonConvert.DeserializeObject<UPluginModel>(upluginText);

        if (uPlugin?.Modules == null || uPlugin.Modules.Count == 0)
        {
            Models.ModuleModel module = new();
            module.Name = SelectedMod.ToString();
            module.Type = "Runtime";
            module.LoadingPhase = "Default";
            uPlugin.Modules ??= new ObservableCollection<ModuleModel>();
            uPlugin.Modules.Add(module);
            upluginText = JsonConvert.SerializeObject(uPlugin, Formatting.Indented);
            File.WriteAllText(upluginPath, upluginText);

        }

        _processService.AddStringToOutput(StringHelper.ModuleFilesGenerated);
        _appNotificationService.SendNotification(StringHelper.ModuleFilesGenerated);
    }

    public async void PerformAddBPFL(string className)
    {

        await PerformGenerateModuleFiles("False"); //Should probably do some sort of checks

        var hFile = await ResourceHelpers.GetTemplateResourceAndReplace("BPFL.h.txt", "[ClassName]", className);
        var cppFile = await ResourceHelpers.GetTemplateResourceAndReplace("BPFL.cpp.txt", "[ClassName]", className);

        hFile = hFile.Replace("[ModReferenceUC]", SelectedMod.ToString().ToUpper());

        var headerCreated = _fileService.WriteAllTextIfNew($"{publicDir}//{className}.h", hFile);
        var cppCreated = _fileService.WriteAllTextIfNew($"{privateDir}//{className}.cpp", cppFile);

        if (headerCreated && cppCreated)
        {
            _appNotificationService.SendNotification($"BPFL {className} has been created");
            _processService.AddStringToOutput($"BPFL {className} created");
        }
        else
        {
            _appNotificationService.SendNotification($"Unable to create BPFL {className}. Class already exists");
            _processService.AddStringToOutput($"Unable to create BPFL {className}. Class already exists");
        }
    }

    public async void PerformAddSubsystem(string className)
    {
        await PerformGenerateModuleFiles("False"); //Should probably do some sort of checks

        var hFile = await ResourceHelpers.GetTemplateResourceAndReplace("Subsystem.h.txt", "[ClassName]", className);
        var cppFile = await ResourceHelpers.GetTemplateResourceAndReplace("Subsystem.cpp.txt", "[ClassName]", className);

        hFile = hFile.Replace("[ModReferenceUC]", SelectedMod.ToString().ToUpper());

        var headerCreated = _fileService.WriteAllTextIfNew($"{publicDir}//{className}.h", hFile);
        var cppCreated = _fileService.WriteAllTextIfNew($"{privateDir}//{className}.cpp", cppFile);

        if (headerCreated && cppCreated)
        {
            _appNotificationService.SendNotification($"Subsystem {className} has been created");
            _processService.AddStringToOutput($"Subsystem {className} created");
        }
        else
        {
            _appNotificationService.SendNotification($"Unable to create Subsystem {className}. Class already exists");
            _processService.AddStringToOutput($"Unable to create Subsystem {className}. Class already exists");
        }
    }

    public async void PerformAddRCO(string className)
    {
        await PerformGenerateModuleFiles("False"); //Should probably do some sort of checks

        var hFile = await ResourceHelpers.GetTemplateResourceAndReplace("RCO.h.txt", "[ClassName]", className);
        var cppFile = await ResourceHelpers.GetTemplateResourceAndReplace("RCO.cpp.txt", "[ClassName]", className);

        hFile = hFile.Replace("[ModReferenceUC]", SelectedMod.ToString().ToUpper());

        var headerCreated = _fileService.WriteAllTextIfNew($"{publicDir}//{className}.h", hFile);
        var cppCreated = _fileService.WriteAllTextIfNew($"{privateDir}//{className}.cpp", cppFile);

        if (headerCreated && cppCreated)
        {
            _appNotificationService.SendNotification($"RCO {className} has been created");
            _processService.AddStringToOutput($"RCO {className} created");
        }
        else
        {
            _appNotificationService.SendNotification($"Unable to create RCO {className}. Class already exists");
            _processService.AddStringToOutput($"Unable to create RCO {className}. Class already exists");
        }
    }

    private AsyncRelayCommand buildForDevelopmentEditor;
    public ICommand BuildForDevelopmentEditor => buildForDevelopmentEditor ??= new AsyncRelayCommand(PerformBuildForDevelopmentEditor);
    private async Task PerformBuildForDevelopmentEditor()
    {
        await _processService.RunBuild(false, _settingsService.Settings.UnrealEngineFolderPath, _settingsService.Settings.UProjectFilePath);
        _appNotificationService.SendNotification(StringHelper.BuildDevComplete);
    }

    private AsyncRelayCommand buildForShipping;
    public ICommand BuildForShipping => buildForShipping ??= new AsyncRelayCommand(PerformBuildForShipping);
    private async Task PerformBuildForShipping()
    {
        await _processService.RunBuild(true, _settingsService.Settings.UnrealEngineFolderPath, _settingsService.Settings.UProjectFilePath);
        _appNotificationService.SendNotification(StringHelper.BuildShippingComplete);

        if (CopyDLLAfterBuildShipping)
        {
            await PerformCopyCPPFiles();
        }
    }

    private AsyncRelayCommand copyCPPFiles;
    public ICommand CopyCPPFiles => copyCPPFiles ??= new AsyncRelayCommand(PerformCopyCPPFiles);
    private async Task PerformCopyCPPFiles()
    {
        //F:\SatisfactoryModMaking\SML-master\Mods\CounterLimiter\Binaries\Win64
        //FactoryGame-CounterLimiter-Win64-Shipping.dll
        //FactoryGame-CounterLimiter-Win64-Shipping.pdb
        //FactoryGame-Win64-Shipping.modules
        //F:\Games\SteamLibrary\steamapps\common\Satisfactory\FactoryGame\Mods\CounterLimiter\Binaries\Win64
        var modBinariesLocation = $"{_settingsService.Settings.UProjectFolderPath}\\Mods\\{SelectedMod}\\Binaries\\Win64";

        var dllFileName = $"FactoryGame-{SelectedMod}-Win64-Shipping.dll";
        var pdbFileName = $"FactoryGame-{SelectedMod}-Win64-Shipping.pdb";

        var dllSource = Path.Combine(modBinariesLocation, dllFileName);
        var pdbSource = Path.Combine(modBinariesLocation, pdbFileName);

        var modGameLocation = $"{_settingsService.Settings.SatisfactoryFolderPath}\\FactoryGame\\Mods\\{SelectedMod}\\Binaries\\Win64";

        var dllDest = Path.Combine(modGameLocation, dllFileName);
        var pdbDest = Path.Combine(modGameLocation, pdbFileName);

        if (File.Exists(dllSource))
        {
            File.Copy(dllSource, dllDest, overwrite: true);
        }
        if (File.Exists(pdbSource))
        {
            File.Copy(pdbSource, pdbDest, overwrite: true);
        }
        _processService.AddStringToOutput($"DLL and PDB Copied to {modGameLocation}");
        _appNotificationService.SendNotification($"DLL and PDB Copied to Game directory");
    }

    public DataGrid OutputDataGrid;

    private AsyncRelayCommand clearOutput;
    public ICommand ClearOutput => clearOutput ??= new AsyncRelayCommand(PerformClearOutput);
    private async Task PerformClearOutput()
    {
        var path = Path.GetDirectoryName(Environment.ProcessPath) + "\\ProcessLog.txt";
        File.WriteAllText(path, "");
        OutputText.Clear();
    }

    private ObservableCollection<string> outputText = new();
    public ObservableCollection<string> OutputText
    {
        get => outputText;
        set => SetProperty(ref outputText, value);
    }

    private long lastFileLocation = 0;

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
}
