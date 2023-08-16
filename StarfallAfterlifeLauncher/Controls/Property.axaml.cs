using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class Property : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<Property, string>(nameof(Text));

        public string Text
        {
            get { return GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public Property()
        {
            InitializeComponent();
        }
    }
}
