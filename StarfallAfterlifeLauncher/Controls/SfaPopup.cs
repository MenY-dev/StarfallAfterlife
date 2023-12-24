using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Metadata;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Launcher.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    [PseudoClasses(":show")]
    public class SfaPopup : Window
    {
        protected override Type StyleKeyOverride => typeof(SfaPopup);

        public Task ShowDialog()
        {
            var root = App.MainWindow;

            if (root is null)
                return Task.CompletedTask;

            return ShowDialog(root);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            PseudoClasses.Set(":show", true);
        }

        protected override void OnClosed(EventArgs e)
        {
            PseudoClasses.Set(":show", false);
            base.OnClosed(e);
        }

        public override void Show()
        {
            base.Show();
            PseudoClasses.Set(":show", true);
        }

        public override void Hide()
        {
            PseudoClasses.Set(":show", false);
            base.Hide();
        }
    }
}
