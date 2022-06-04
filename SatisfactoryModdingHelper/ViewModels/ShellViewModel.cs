using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using MahApps.Metro.Controls;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using PeanutButter.TinyEventAggregator;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Models;
using SatisfactoryModdingHelper.Properties;
using SatisfactoryModdingHelper.Services;

namespace SatisfactoryModdingHelper.ViewModels
{
    public class ShellViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private readonly IPluginService _pluginService;
        private readonly IFileService _fileService;
        private readonly EventAggregator _eventAggregator;
        private HamburgerMenuItem _selectedMenuItem;
        private HamburgerMenuItem _selectedOptionsMenuItem;
        private RelayCommand _goBackCommand;
        private ICommand _menuItemInvokedCommand;
        private ICommand _optionsMenuItemInvokedCommand;
        private ICommand _loadedCommand;
        private ICommand _unloadedCommand;

        public HamburgerMenuItem SelectedMenuItem
        {
            get { return _selectedMenuItem; }
            set { SetProperty(ref _selectedMenuItem, value); }
        }

        public HamburgerMenuItem SelectedOptionsMenuItem
        {
            get { return _selectedOptionsMenuItem; }
            set { SetProperty(ref _selectedOptionsMenuItem, value); }
        }

        // TODO WTS: Change the icons and titles for all HamburgerMenuItems here.
        public ObservableCollection<HamburgerMenuItem> MenuItems { get; } = new ObservableCollection<HamburgerMenuItem>()
        {
            new HamburgerMenuGlyphItem() { Label = Resources.ShellMainPage, Glyph = "\uE1CF", TargetPageType = typeof(MainViewModel) },
            new HamburgerMenuGlyphItem() { Label = Resources.ShellUPluginPage, Glyph = "\uE95F", TargetPageType = typeof(UPluginViewModel) },
            new HamburgerMenuGlyphItem() { Label = Resources.ShellCPPPage, Glyph = "\uF133", TargetPageType = typeof(CppViewModel) },
            new HamburgerMenuGlyphItem() { Label = Resources.ShellAccessTransformersPage, Glyph = "\uE8B1", TargetPageType = typeof(AccessTransformersViewModel) },
        };

        public ObservableCollection<HamburgerMenuItem> OptionMenuItems { get; } = new ObservableCollection<HamburgerMenuItem>()
        {
            new HamburgerMenuGlyphItem() { Label = Resources.ShellSettingsPage, Glyph = "\uE713", TargetPageType = typeof(SettingsViewModel) }
        };

        public RelayCommand GoBackCommand => _goBackCommand ??= new RelayCommand(OnGoBack, CanGoBack);

        public ICommand MenuItemInvokedCommand => _menuItemInvokedCommand ??= new RelayCommand(OnMenuItemInvoked);

        public ICommand OptionsMenuItemInvokedCommand => _optionsMenuItemInvokedCommand ??= new RelayCommand(OnOptionsMenuItemInvoked);

        public ICommand LoadedCommand => _loadedCommand ??= new RelayCommand(OnLoaded);

        public ICommand UnloadedCommand => _unloadedCommand ??= new RelayCommand(OnUnloaded);

        public ShellViewModel(INavigationService navigationService, IPersistAndRestoreService persistAndRestoreService, IPluginService pluginService, IFileService fileService)
        {
            _navigationService = navigationService;
            _persistAndRestoreService = persistAndRestoreService;
            _pluginService = pluginService;
            _fileService = fileService;
            _eventAggregator = EventAggregator.Instance;
        }

        private void OnLoaded()
        {
            _navigationService.Navigated += OnNavigated;
            if (_persistAndRestoreService.Settings.AppWidth != null)
            {
                System.Windows.Application.Current.MainWindow.Width = (double)_persistAndRestoreService.Settings.AppWidth;
            }
            if (_persistAndRestoreService.Settings.AppHeight != null)
            {
                System.Windows.Application.Current.MainWindow.Height = (double)_persistAndRestoreService.Settings.AppHeight;
            }

            SelectedPlugin = _pluginService.SelectedPlugin;
            PluginList = _pluginService.PluginList;
        }

        private void OnUnloaded()
        {
            _navigationService.Navigated -= OnNavigated;

            _pluginService.SelectedPlugin = SelectedPlugin;
            _persistAndRestoreService.PersistData();
        }

        public void SaveNewSize()
        {
            _persistAndRestoreService.Settings.AppWidth = App.Current.MainWindow.Width;
            _persistAndRestoreService.Settings.AppHeight = App.Current.MainWindow.Height;
            _persistAndRestoreService.PersistData();
        }

        private bool CanGoBack()
            => _navigationService.CanGoBack;

        private void OnGoBack()
            => _navigationService.GoBack();

        private void OnMenuItemInvoked()
            => NavigateTo(SelectedMenuItem.TargetPageType);

        private void OnOptionsMenuItemInvoked()
            => NavigateTo(SelectedOptionsMenuItem.TargetPageType);

        private void NavigateTo(Type targetViewModel)
        {
            if (targetViewModel != null)
            {
                _navigationService.NavigateTo(targetViewModel.FullName);
            }
        }

        private void OnNavigated(object sender, string viewModelName)
        {
            var item = MenuItems
                        .OfType<HamburgerMenuItem>()
                        .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
            if (item != null)
            {
                SelectedMenuItem = item;
            }
            else
            {
                SelectedOptionsMenuItem = OptionMenuItems
                        .OfType<HamburgerMenuItem>()
                        .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
            }

            SetViewTitle(viewModelName);
            SetPluginComboBoxVisibility(viewModelName);
            GoBackCommand.NotifyCanExecuteChanged();
        }

        private void SetPluginComboBoxVisibility(string viewModelName)
        {
            switch (viewModelName)
            {
                case "SatisfactoryModdingHelper.ViewModels.MainViewModel":
                    PluginSelectorVisibility = System.Windows.Visibility.Visible;
                    break;
                case "SatisfactoryModdingHelper.ViewModels.UPluginViewModel":
                    PluginSelectorVisibility = System.Windows.Visibility.Visible;
                    break;
                case "SatisfactoryModdingHelper.ViewModels.CppViewModel":
                    PluginSelectorVisibility = System.Windows.Visibility.Visible;
                    break;
                case "SatisfactoryModdingHelper.ViewModels.AccessTransformersViewModel":
                    PluginSelectorVisibility = System.Windows.Visibility.Visible;
                    break;
                case "SatisfactoryModdingHelper.ViewModels.SettingsViewModel":
                    PluginSelectorVisibility = System.Windows.Visibility.Collapsed;
                    break;
            }
        }

        private void SetViewTitle(string viewModelName)
        {
            switch (viewModelName)
            {
                case "SatisfactoryModdingHelper.ViewModels.MainViewModel":
                    ViewTitle = Resources.MainPageTitle;
                    break;
                case "SatisfactoryModdingHelper.ViewModels.UPluginViewModel":
                    ViewTitle = Resources.UPluginPageTitle;
                    break;
                case "SatisfactoryModdingHelper.ViewModels.CppViewModel":
                    ViewTitle = Resources.CPPPageTitle;
                    break;
                case "SatisfactoryModdingHelper.ViewModels.AccessTransformersViewModel":
                    ViewTitle = Resources.AccessTransformersPageTitle;
                    break;
                case "SatisfactoryModdingHelper.ViewModels.SettingsViewModel":
                    ViewTitle = Resources.SettingsPageTitle;
                    break;
            }
        }

        private string viewTitle;

        public string ViewTitle { get => viewTitle; set => SetProperty(ref viewTitle, value); }

        private bool pluginComboBoxEnabled;

        public bool PluginComboBoxEnabled { get => pluginComboBoxEnabled; set => SetProperty(ref pluginComboBoxEnabled, value); }

        private System.Collections.IEnumerable pluginList;

        public System.Collections.IEnumerable PluginList { get => pluginList; set => SetProperty(ref pluginList, value); }

        private object selectedPlugin;

        public object SelectedPlugin { get => selectedPlugin;
            set
            {
                if (SetProperty(ref selectedPlugin, value))
                {
                    _pluginService.SelectedPlugin = value;
                    _eventAggregator.GetEvent<PluginSelectedEvent>().Publish(SelectedPlugin);
                    _persistAndRestoreService.Settings.CurrentPlugin = value.ToString();
                    _persistAndRestoreService.PersistData();
                }
            }
        }

        private System.Windows.Visibility pluginSelectorVisibility;

        public System.Windows.Visibility PluginSelectorVisibility { get => pluginSelectorVisibility; set => SetProperty(ref pluginSelectorVisibility, value); }
    }
}
