using Microsoft.UI.Xaml.Controls;

using SatisfactoryModdingHelper.ViewModels;

namespace SatisfactoryModdingHelper.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    private void ScrollViewer_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
    {
        var sv = (ScrollViewer)sender;
        sv.ChangeView(null, sv.ScrollableHeight, null);
    }
}
