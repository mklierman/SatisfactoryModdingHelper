using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Core.Contracts.Services;
using SatisfactoryModdingHelper.Models;

namespace SatisfactoryModdingHelper.ViewModels;

public class AccessTransformersViewModel : ObservableRecipient, INavigationAware
{
    private readonly ILocalSettingsService _localSettingsService;
    private readonly IPluginService _pluginService;
    private readonly IFileService _fileService;
    //                                        ^(\w+)\S*(?:Class=")(\w+)\S+\s\S*(?:=")(\w+)
    private const string regexPattern = "^(\\w+)\\S*(?:Class=\")(\\w+)\\S+\\s\\S*(?:=\")(\\w+)";
    private string projectDirectory;
    private ObservableCollection<AccessorModel> accessors;
    private ObservableCollection<FriendModel> friends;
    private ObservableCollection<BlueprintReadWriteModel> blueprints;

    public AccessTransformersViewModel(IPluginService pluginService, IFileService fileService, ILocalSettingsService localSettingsService)
    {
        _pluginService = pluginService;
        _fileService = fileService;
        _localSettingsService = localSettingsService;
    }
    public void OnNavigatedFrom()
    {
        _pluginService.PluginChangedEvent -= OnPluginChanged;
    }

    public void OnNavigatedTo(object parameter)
    {
        projectDirectory = _localSettingsService.Settings.ProjectPath;
        SelectedPlugin = _pluginService.SelectedPlugin;
        PopulateAccessTransformerFields();
        _pluginService.PluginChangedEvent += OnPluginChanged;
    }

    private void OnPluginChanged(object? sender, object e)
    {
        SelectedPlugin = e;
    }


    private object selectedPlugin;
    public object SelectedPlugin
    {
        get => selectedPlugin;
        set
        {
            if (SetProperty(ref selectedPlugin, value))
            {
                PopulateAccessTransformerFields();
            }
        }
    }

    public PivotItem SelectedPivotItem
    {
        get;
        set;
    }

    public ObservableCollection<AccessorModel> Accessors
    {
        get => accessors;
        set => SetProperty(ref accessors, value);
    }

    public ObservableCollection<FriendModel> Friends
    {
        get => friends;
        set => SetProperty(ref friends, value);
    }

    public ObservableCollection<BlueprintReadWriteModel> Blueprints
    {
        get => blueprints;
        set => SetProperty(ref blueprints, value);
    }


    private AccessTransformersModel loadedAccessTransformers;
    public AccessTransformersModel LoadedAccessTransformers
    {
        get => loadedAccessTransformers;
        set => SetProperty(ref loadedAccessTransformers, value);
    }

    private AccessTransformersModel GetSelectedPluginAccessTransformers()
    {
        var returnModel = new AccessTransformersModel();

        if (string.IsNullOrEmpty(projectDirectory) || SelectedPlugin == null)
        {
            return returnModel;
        }

        string filePath = @$"{projectDirectory}\Plugins\{SelectedPlugin}\Config\AccessTransformers.ini";
        if (!File.Exists(filePath))
        {
            return returnModel;
        }

        var fileContents = File.ReadAllLines(path: filePath);
        foreach (var line in fileContents)
        {
            var matches = Regex.Matches(line, regexPattern, RegexOptions.Singleline);
            if (matches.Count > 0)
            {
                var transformType = matches[0].Groups[1].Value.Trim();
                switch (transformType)
                {
                    case "Friend":
                        returnModel.FriendTransformers.Add(new FriendModel()
                        {
                            Class = matches[0].Groups[2].Value.Trim(),
                            FriendClass = matches[0].Groups[3].Value.Trim()
                        });
                        break;
                    case "Accessor":
                        returnModel.AccessorTransformers.Add(new AccessorModel()
                        {
                            Class = matches[0].Groups[2].Value.Trim(),
                            Property = matches[0].Groups[3].Value.Trim()
                        });
                        break;
                    case "BlueprintReadWrite":
                        returnModel.BlueprintReadWriteTransformers.Add(new BlueprintReadWriteModel()
                        {
                            Class = matches[0].Groups[2].Value.Trim(),
                            Property = matches[0].Groups[3].Value.Trim()
                        });
                        break;
                }

            }
        }
        return returnModel;
    }

    private void PopulateAccessTransformerFields()
    {
        LoadedAccessTransformers = GetSelectedPluginAccessTransformers();
        Accessors = LoadedAccessTransformers.AccessorTransformers;
        Friends = LoadedAccessTransformers.FriendTransformers;
        Blueprints = LoadedAccessTransformers.BlueprintReadWriteTransformers;
    }

    private RelayCommand addTransformer;
    public ICommand AddTransformer => addTransformer ??= new RelayCommand(PerformAddTransformer);

    private void PerformAddTransformer()
    {
        switch (SelectedPivotItem.Header?.ToString())
        {
            case "Friends":
                Friends.Add(new FriendModel());
                break;
            case "Blueprint Read/Writes":
                Blueprints.Add(new BlueprintReadWriteModel());
                break;
            case "Accessors":
                Accessors.Add(new AccessorModel());
                break;
        }
    }

    private RelayCommand addFriend;
    public ICommand AddFriend => addFriend ??= new RelayCommand(PerformAddFriend);

    private void PerformAddFriend()
    {
        Friends.Add(new FriendModel());
    }

    private RelayCommand<FriendModel> removeFriend;
    public ICommand RemoveFriend => removeFriend ??= new RelayCommand<FriendModel>(param => this.PerformRemoveFriend(param));

    private void PerformRemoveFriend(FriendModel friendModel)
    {
        Friends.Remove(friendModel);
    }

    private RelayCommand addAccessor;
    public ICommand AddAccessor => addAccessor ??= new RelayCommand(PerformAddAccessor);

    private void PerformAddAccessor()
    {
        Accessors.Add(new AccessorModel());
    }
    private RelayCommand<AccessorModel> removeAccessor;
    public ICommand RemoveAccessor => removeAccessor ??= new RelayCommand<AccessorModel>(param => this.PerformRemoveAccessor(param));
    private void PerformRemoveAccessor(AccessorModel accessorModel)
    {
        Accessors.Remove(accessorModel);
    }

    private RelayCommand addBlueprintReadWrite;
    public ICommand AddBlueprintReadWrite => addBlueprintReadWrite ??= new RelayCommand(PerformAddBlueprintReadWrite);

    private void PerformAddBlueprintReadWrite()
    {
        Blueprints.Add(new BlueprintReadWriteModel());
    }
    private RelayCommand<BlueprintReadWriteModel> removeBlueprintReadWrite;
    public ICommand RemoveBlueprintReadWrite => removeBlueprintReadWrite ??= new RelayCommand<BlueprintReadWriteModel>(param => this.PerformRemoveBlueprintReadWrite(param));

    private void PerformRemoveBlueprintReadWrite(BlueprintReadWriteModel blueprintReadWriteModel)
    {
        Blueprints.Remove(blueprintReadWriteModel);
    }

    private RelayCommand saveChanges;
    public ICommand SaveChanges => saveChanges ??= new RelayCommand(PerformSaveChanges);

    private void PerformSaveChanges()
    {
        if (string.IsNullOrEmpty(projectDirectory) || SelectedPlugin == null)
        {
            return;
        }

        var folderPath = @$"{projectDirectory}\Plugins\{SelectedPlugin}\Config\";
        var fileName = $@"AccessTransformers.ini";

        AccessTransformersModel accessTransformers = new AccessTransformersModel()
        {
            AccessorTransformers = Accessors,
            FriendTransformers = Friends,
            BlueprintReadWriteTransformers = Blueprints
        };

        _fileService.SaveAccessTransformers(folderPath, fileName, accessTransformers);
    }

    private RelayCommand cancelChanges;
    public ICommand CancelChanges => cancelChanges ??= new RelayCommand(PerformCancelChanges);

    private void PerformCancelChanges()
    {
        PopulateAccessTransformerFields();
    }
}
