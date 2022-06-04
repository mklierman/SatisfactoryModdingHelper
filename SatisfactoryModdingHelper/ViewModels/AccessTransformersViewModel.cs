using System;
using System.IO;
using IniParser.Model;
using IniParser;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using PeanutButter.TinyEventAggregator;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Contracts.ViewModels;
using SatisfactoryModdingHelper.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Windows.System;
using System.Collections.ObjectModel;

namespace SatisfactoryModdingHelper.ViewModels
{
    public class AccessTransformersViewModel : ObservableObject, INavigationAware
    {
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private readonly IPluginService _pluginService;
        private readonly IFileService _fileService;
        private readonly EventAggregator _eventAggregator;
        //                                        ^(\w+)\S*(?:Class=")(\w+)\S+\s\S*(?:=")(\w+)
        private const string regexPattern = "^(\\w+)\\S*(?:Class=\")(\\w+)\\S+\\s\\S*(?:=\")(\\w+)";
        private SubscriptionToken pluginSelectedToken;
        private string projectDirectory;
        private ObservableCollection<AccessorModel> accessors;
        private ObservableCollection<FriendModel> friends;
        private ObservableCollection<BlueprintReadWriteModel> blueprints;

        public AccessTransformersViewModel(IPersistAndRestoreService persistAndRestoreService, IPluginService pluginService, IFileService fileService)
        {
            _persistAndRestoreService = persistAndRestoreService;
            _pluginService = pluginService;
            _fileService = fileService;
            _eventAggregator = EventAggregator.Instance;
        }

        public void OnNavigatedFrom()
        {
            _eventAggregator.GetEvent<PluginSelectedEvent>().Unsubscribe(pluginSelectedToken);
        }

        public void OnNavigatedTo(object parameter)
        {
            projectDirectory = _persistAndRestoreService.Settings.ProjectPath;
            SelectedPlugin = _pluginService.SelectedPlugin;
            PopulateAccessTransformerFields();
            pluginSelectedToken = _eventAggregator.GetEvent<PluginSelectedEvent>().Subscribe(PluginSelected);
        }

        public void OnStartingNavigateFrom()
        {
        }
        internal void PluginSelected(object plugin)
        {
            SelectedPlugin = _pluginService.SelectedPlugin;
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

            string folderPath = @$"{projectDirectory}\Plugins\{SelectedPlugin}\Config\";
            string fileName = $@"AccessTransformers.ini";

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
}
