using Microsoft.UI.Xaml.Controls;

using SatisfactoryModdingHelper.ViewModels;

namespace SatisfactoryModdingHelper.Views;

public sealed partial class AccessTransformersPage : Page
{
    public AccessTransformersViewModel ViewModel
    {
        get;
    }

    public AccessTransformersPage()
    {
        ViewModel = App.GetService<AccessTransformersViewModel>();
        InitializeComponent();
    }
}
