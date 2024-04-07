using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class SfaPanel : UserControl
    {
        public static readonly DirectProperty<SfaPanel, bool> ShowHeaderProperty =
            AvaloniaProperty.RegisterDirect<SfaPanel, bool>(
                nameof(ShowHeader),
                o => o.ShowHeader,
                (o, v) => o.ShowHeader = v,
                unsetValue: false,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly DirectProperty<SfaPanel, string> HeaderTextProperty =
            AvaloniaProperty.RegisterDirect<SfaPanel, string>(
                nameof(HeaderText),
                o => o.HeaderText,
                (o, v) => o.HeaderText = v,
                unsetValue: null,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<IBrush> AccentColorProperty =
            AvaloniaProperty.Register<SfaPanel, IBrush>(nameof(AccentColor));

        public bool ShowHeader
        {
            get => showHeader;
            set
            {
                SetAndRaise(ShowHeaderProperty, ref showHeader, value);
            }
        }

        public string HeaderText
        {
            get => headerText;
            set
            {
                SetAndRaise(HeaderTextProperty, ref headerText, value);
            }
        }

        public IBrush AccentColor { get => GetValue(AccentColorProperty); set => SetValue(AccentColorProperty, value); }

        private bool showHeader = false;
        private string headerText = null;
    }
}
