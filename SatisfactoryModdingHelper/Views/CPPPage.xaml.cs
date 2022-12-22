using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SatisfactoryModdingHelper.Dialogs;
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
       // ViewModel.OutputDataGrid = OutputDataGrid;
    }

    private async void BPFLButton_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new ContentDialog();
        dialog.XamlRoot = XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "Class Name";
        dialog.PrimaryButtonText = "Accept";
        dialog.CloseButtonText = "Cancel";
        dialog.DefaultButton = ContentDialogButton.Primary;
        dialog.Content = new SingleInputRequestDialog("Name for Blueprint Function Library class: ");
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.PerformAddBPFL(((SingleInputRequestDialog)dialog.Content).InputResult);
        }
    }

    private async void SubsystemButton_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new ContentDialog();
        dialog.XamlRoot = XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "Class Name";
        dialog.PrimaryButtonText = "Accept";
        dialog.CloseButtonText = "Cancel";
        dialog.DefaultButton = ContentDialogButton.Primary;
        dialog.Content = new SingleInputRequestDialog("Name for Subsystem class: ");
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.PerformAddSubsystem(((SingleInputRequestDialog)dialog.Content).InputResult);
        }
    }

    private async void RcoButton_Click(object sender, RoutedEventArgs e)
    {

        ContentDialog dialog = new ContentDialog();
        dialog.XamlRoot = XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "Class Name";
        dialog.PrimaryButtonText = "Accept";
        dialog.CloseButtonText = "Cancel";
        dialog.DefaultButton = ContentDialogButton.Primary;
        dialog.Content = new SingleInputRequestDialog("Name for RCO class: ");
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.PerformAddRCO(((SingleInputRequestDialog)dialog.Content).InputResult);
        }
    }

    private void DataGrid_SizeChanged(object sender, DataGridRowEventArgs e)
    {
        Console.WriteLine("Loading Row");
        //if (ScrollToggle.IsEnabled)
        //{
        //    var dg = (DataGrid)sender;
        //    if (((ObservableCollection<string>)dg.ItemsSource).Count() > 1)
        //    {
        //        var firstItem = ((ObservableCollection<string>)dg.ItemsSource).First();
        //        var lastItem = ((ObservableCollection<string>)dg.ItemsSource).Last();
        //        if (firstItem != lastItem)
        //        {
        //          dg.ScrollIntoView(lastItem, dg.Columns[0]);
        //        }
        //    }
        //}
    }

    private void DataGrid_SizeChanged_1(object sender, SizeChangedEventArgs e)
    {

        Console.WriteLine("SizeChanged");
    }
}
