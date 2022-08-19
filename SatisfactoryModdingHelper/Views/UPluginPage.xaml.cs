using Microsoft.UI.Xaml.Controls;

using SatisfactoryModdingHelper.ViewModels;

namespace SatisfactoryModdingHelper.Views;

public sealed partial class UPluginPage : Page
{
    public UPluginViewModel ViewModel
    {
        get;
    }

    public UPluginPage()
    {
        ViewModel = App.GetService<UPluginViewModel>();
        InitializeComponent();
    }
}
