using System.Windows.Controls;

namespace SatisfactoryModdingHelper.Contracts.Views
{
    public interface IShellWindow
    {
        Frame GetNavigationFrame();

        void ShowWindow();

        void CloseWindow();
    }
}
