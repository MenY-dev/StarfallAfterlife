using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using StarfallAfterlife.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StarfallAfterlife.Launcher.Controls
{
    public class SfaWindow : Window
    {
        protected override Type StyleKeyOverride => typeof(SfaWindow);

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        public override void BeginInit()
        {
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            ExtendClientAreaTitleBarHeightHint = -1;
            base.BeginInit();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change is not null &&
                change.Property == OffScreenMarginProperty &&
                change.NewValue is Thickness padding)
            {
                Padding = padding;
            }
        }

        public void OnCloseBtnClicked()
        {
            Close();
        }

        public void OnMaxMinBtnClicked()
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
        }

        public void OnHideBtnClicked()
        {
            WindowState = WindowState.Minimized;
        }

        public void OnWindowDragStarted(PointerPressedEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                OnMaxMinBtnClicked();
            }

            BeginMoveDrag(e);
        }
    }
}
