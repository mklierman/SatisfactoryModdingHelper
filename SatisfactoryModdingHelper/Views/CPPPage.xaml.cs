using System.Windows.Controls;

using SatisfactoryModdingHelper.ViewModels;

namespace SatisfactoryModdingHelper.Views
{
    public partial class CPPPage : Page
    {
        public CPPPage(CPPViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
