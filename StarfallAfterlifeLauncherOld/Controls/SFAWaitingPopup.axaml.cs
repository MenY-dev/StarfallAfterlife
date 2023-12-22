using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class SFAWaitingPopup : Window
    {
        public SFAWaitingPopup()
        {
            InitializeComponent();
        }

        public Task ShowDialog()
        {
            return ShowDialog((App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
        }
    }
}
