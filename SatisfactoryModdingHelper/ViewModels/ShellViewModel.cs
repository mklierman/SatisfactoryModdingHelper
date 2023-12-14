using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private readonly IModService _modService;

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

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService, IModService modService, ILocalSettingsService settingsService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
        _localSettingsService = settingsService;
        _modService = modService;
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
        ModList = _modService.ModList;
        SelectedMod = _modService.SelectedMod;
    }

    private bool modComboBoxEnabled;

    public bool ModComboBoxEnabled
    {
        get => modComboBoxEnabled; set => SetProperty(ref modComboBoxEnabled, value);
    }

    private System.Collections.IEnumerable modList;

    public System.Collections.IEnumerable ModList
    {
        get => modList; set => SetProperty(ref modList, value);
    }

    private AsyncRelayCommand refreshList;
    public ICommand RefreshList => refreshList ??= new AsyncRelayCommand(PerformRefreshList);
    private async Task PerformRefreshList()
    {
        var curMod = SelectedMod;
        ModList = _modService.ModList;
        SelectedMod = curMod;
    }

    private object selectedMod;

    public object SelectedMod
    {
        get => selectedMod;
        set
        {
            SetProperty(ref selectedMod, value);
            if (value != null)
            {

                _modService.SelectedMod = value;
                _localSettingsService.Settings.CurrentMod = value.ToString();
                _localSettingsService.PersistData();
            }
        }
    }

    private Visibility modSelectorVisibility;

    public Visibility ModSelectorVisibility
    {
        get => modSelectorVisibility; set => SetProperty(ref modSelectorVisibility, value);
    }

}
