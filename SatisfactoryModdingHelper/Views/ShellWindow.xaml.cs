using System.Windows.Controls;

using MahApps.Metro.Controls;

using SatisfactoryModdingHelper.Contracts.Views;
using SatisfactoryModdingHelper.ViewModels;

namespace SatisfactoryModdingHelper.Views
{
    public partial class ShellWindow : MetroWindow, IShellWindow
    {
        public ShellWindow(ShellViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public Frame GetNavigationFrame()
            => shellFrame;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        private void MetroWindow_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            ShellViewModel vm = (ShellViewModel)DataContext;
            vm.SaveNewSize();
        }
    }
}
