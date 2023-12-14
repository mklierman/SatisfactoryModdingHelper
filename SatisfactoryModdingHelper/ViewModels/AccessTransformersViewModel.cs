using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Core.Contracts.Services;
using SatisfactoryModdingHelper.Models;

namespace SatisfactoryModdingHelper.ViewModels;

public class AccessTransformersViewModel : ObservableRecipient, INavigationAware
{
    private readonly ILocalSettingsService _localSettingsService;
    private readonly IModService _modService;
    private readonly IFileService _fileService;
    //                                        ^(\w+)\S*(?:Class=")(\w+)\S+\s\S*(?:=")(\w+)
    private const string regexPattern = "^(\\w+)\\S*(?:Class=\")(\\w+)\\S+\\s\\S*(?:=\")(\\w+)";
    private string projectDirectory;
    private ObservableCollection<AccessorModel> accessors;
    private ObservableCollection<FriendModel> friends;
    private ObservableCollection<BlueprintReadWriteModel> blueprints;

    public AccessTransformersViewModel(IModService modService, IFileService fileService, ILocalSettingsService localSettingsService)
    {
        _modService = modService;
        _fileService = fileService;
        _localSettingsService = localSettingsService;
    }
    public void OnNavigatedFrom()
    {
        _modService.ModChangedEvent -= OnModChanged;
    }

    public void OnNavigatedTo(object parameter)
    {
        projectDirectory = _localSettingsService.Settings.UProjectFolderPath;
        SelectedMod = _modService.SelectedMod;
        PopulateAccessTransformerFields();
        _modService.ModChangedEvent += OnModChanged;
    }

    private void OnModChanged(object? sender, object e)
    {
        SelectedMod = e;
    }

    private bool unsavedChanges = false;
    public bool UnsavedChanges
    {
        get => unsavedChanges;
        set
        {
            SetProperty(ref unsavedChanges, value);
            SaveCancelVisibility =value ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private Visibility saveCancelVisibility = Visibility.Collapsed;
    public Visibility SaveCancelVisibility
    {
        get => saveCancelVisibility;
        set => SetProperty(ref saveCancelVisibility, value);
    }


    private object selectedMod;
    public object SelectedMod
    {
        get => selectedMod;
        set
        {
            if (SetProperty(ref selectedMod, value))
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

    private AccessTransformersModel GetSelectedModAccessTransformers()
    {
        var returnModel = new AccessTransformersModel();

        if (string.IsNullOrEmpty(projectDirectory) || SelectedMod == null)
        {
            return returnModel;
        }

        string filePath = @$"{projectDirectory}\Mods\{SelectedMod}\Config\AccessTransformers.ini";
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
        LoadedAccessTransformers = GetSelectedModAccessTransformers();
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
        UnsavedChanges = true;
    }

    private RelayCommand<FriendModel> removeFriend;
    public ICommand RemoveFriend => removeFriend ??= new RelayCommand<FriendModel>(param => this.PerformRemoveFriend(param));

    private void PerformRemoveFriend(FriendModel friendModel)
    {
        Friends.Remove(friendModel);
        UnsavedChanges = true;
    }

    private RelayCommand<AccessorModel> removeAccessor;
    public ICommand RemoveAccessor => removeAccessor ??= new RelayCommand<AccessorModel>(param => this.PerformRemoveAccessor(param));
    private void PerformRemoveAccessor(AccessorModel accessorModel)
    {
        Accessors.Remove(accessorModel);
        UnsavedChanges = true;
    }

    private RelayCommand<BlueprintReadWriteModel> removeBlueprintReadWrite;
    public ICommand RemoveBlueprintReadWrite => removeBlueprintReadWrite ??= new RelayCommand<BlueprintReadWriteModel>(param => this.PerformRemoveBlueprintReadWrite(param));

    private void PerformRemoveBlueprintReadWrite(BlueprintReadWriteModel blueprintReadWriteModel)
    {
        Blueprints.Remove(blueprintReadWriteModel);
        UnsavedChanges = true;
    }

    private RelayCommand saveChanges;
    public ICommand SaveChanges => saveChanges ??= new RelayCommand(PerformSaveChanges);

    private void PerformSaveChanges()
    {
        if (string.IsNullOrEmpty(projectDirectory) || SelectedMod == null)
        {
            return;
        }

        var folderPath = @$"{projectDirectory}\Plugins\{SelectedMod}\Config\";
        var fileName = $@"AccessTransformers.ini";

        AccessTransformersModel accessTransformers = new AccessTransformersModel()
        {
            AccessorTransformers = Accessors,
            FriendTransformers = Friends,
            BlueprintReadWriteTransformers = Blueprints
        };

        _fileService.SaveAccessTransformers(folderPath, fileName, accessTransformers);
        UnsavedChanges = false;
    }

    private RelayCommand cancelChanges;
    public ICommand CancelChanges => cancelChanges ??= new RelayCommand(PerformCancelChanges);

    private void PerformCancelChanges()
    {
        PopulateAccessTransformerFields();
        UnsavedChanges = false;
    }
}
