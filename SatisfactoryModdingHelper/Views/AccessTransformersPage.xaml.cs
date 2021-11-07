using System.Windows.Controls;

using SatisfactoryModdingHelper.ViewModels;

namespace SatisfactoryModdingHelper.Views
{
    public partial class AccessTransformersPage : Page
    {
        public AccessTransformersPage(AccessTransformersViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
