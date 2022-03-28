using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using MahApps.Metro.Controls;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Properties;

namespace SatisfactoryModdingHelper.ViewModels
{
    public class ShellViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IPersistAndRestoreService _persistAndRestoreService;
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

        public RelayCommand GoBackCommand => _goBackCommand ?? (_goBackCommand = new RelayCommand(OnGoBack, CanGoBack));

        public ICommand MenuItemInvokedCommand => _menuItemInvokedCommand ?? (_menuItemInvokedCommand = new RelayCommand(OnMenuItemInvoked));

        public ICommand OptionsMenuItemInvokedCommand => _optionsMenuItemInvokedCommand ?? (_optionsMenuItemInvokedCommand = new RelayCommand(OnOptionsMenuItemInvoked));

        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

        public ICommand UnloadedCommand => _unloadedCommand ?? (_unloadedCommand = new RelayCommand(OnUnloaded));

        public ShellViewModel(INavigationService navigationService, IPersistAndRestoreService persistAndRestoreService)
        {
            _navigationService = navigationService;
            _persistAndRestoreService = persistAndRestoreService;
        }

        private void OnLoaded()
        {
            _navigationService.Navigated += OnNavigated;
            if (_persistAndRestoreService.GetSavedProperty(Resources.App_Settings_Width) != null)
            {
                System.Windows.Application.Current.MainWindow.Width = (double)_persistAndRestoreService.GetSavedProperty(Resources.App_Settings_Width);
            }
            if (_persistAndRestoreService.GetSavedProperty(Resources.App_Settings_Height) != null)
            {
                System.Windows.Application.Current.MainWindow.Height = (double)_persistAndRestoreService.GetSavedProperty(Resources.App_Settings_Height);
            }
        }

        private void OnUnloaded()
        {
            _navigationService.Navigated -= OnNavigated;

        }

        public void SaveNewSize()
        {
            _persistAndRestoreService.SaveProperty(Properties.Resources.App_Settings_Width, App.Current.MainWindow.Width);
            _persistAndRestoreService.SaveProperty(Properties.Resources.App_Settings_Height, App.Current.MainWindow.Height);
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

            GoBackCommand.NotifyCanExecuteChanged();
        }
    }
}
