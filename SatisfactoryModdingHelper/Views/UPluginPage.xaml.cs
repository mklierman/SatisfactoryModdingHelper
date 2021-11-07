using System.Windows.Controls;

using SatisfactoryModdingHelper.ViewModels;

namespace SatisfactoryModdingHelper.Views
{
    public partial class UPluginPage : Page
    {
        public UPluginPage(UPluginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
