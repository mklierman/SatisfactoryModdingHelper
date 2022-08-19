using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;

using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Services;
using SatisfactoryModdingHelper.Views;

namespace SatisfactoryModdingHelper.ViewModels;

public class ShellViewModel : ObservableRecipient
{
    private bool _isBackEnabled;
    private object? _selected;
    private readonly ILocalSettingsService _localSettingsService;
    private readonly IPluginService _pluginService;

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService, IPluginService pluginService, ILocalSettingsService settingsService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
        _localSettingsService = settingsService;
        _pluginService = pluginService;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
        PluginList = _pluginService.PluginList;
        SelectedPlugin = _pluginService.SelectedPlugin;
    }

    private bool pluginComboBoxEnabled;

    public bool PluginComboBoxEnabled
    {
        get => pluginComboBoxEnabled; set => SetProperty(ref pluginComboBoxEnabled, value);
    }

    private System.Collections.IEnumerable pluginList;

    public System.Collections.IEnumerable PluginList
    {
        get => pluginList; set => SetProperty(ref pluginList, value);
    }

    private object selectedPlugin;

    public object SelectedPlugin
    {
        get => selectedPlugin;
        set
        {
            SetProperty(ref selectedPlugin, value);
            if (value != null)
            {

                _pluginService.SelectedPlugin = value;
                _localSettingsService.Settings.CurrentPlugin = value.ToString();
                _localSettingsService.PersistData();
            }
        }
    }

    private Visibility pluginSelectorVisibility;

    public Visibility PluginSelectorVisibility
    {
        get => pluginSelectorVisibility; set => SetProperty(ref pluginSelectorVisibility, value);
    }

}
