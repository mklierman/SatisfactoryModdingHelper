using System.Windows.Controls;

using SatisfactoryModdingHelper.ViewModels;

namespace SatisfactoryModdingHelper.Views
{
    public partial class CPPPage : Page
    {
        public CPPPage(CppViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
