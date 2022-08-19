using Microsoft.UI.Xaml.Controls;

using SatisfactoryModdingHelper.ViewModels;

namespace SatisfactoryModdingHelper.Views;

public sealed partial class CPPPage : Page
{
    public CPPViewModel ViewModel
    {
        get;
    }

    public CPPPage()
    {
        ViewModel = App.GetService<CPPViewModel>();
        InitializeComponent();
    }
}
