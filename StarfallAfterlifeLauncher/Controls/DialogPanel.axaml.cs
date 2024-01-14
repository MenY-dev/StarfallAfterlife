using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class DialogPanel : UserControl
    {
        public static readonly StyledProperty<object> HeaderProperty =
            AvaloniaProperty.Register<DialogPanel, object>(nameof(Header));

        public static readonly StyledProperty<object> FooterProperty =
            AvaloniaProperty.Register<DialogPanel, object>(nameof(Footer));

        public static readonly StyledProperty<object> DialogContentProperty =
            AvaloniaProperty.Register<DialogPanel, object>(nameof(DialogContent));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public object Footer
        {
            get => GetValue(FooterProperty);
            set => SetValue(FooterProperty, value);
        }

        [Content]
        public object DialogContent
        {
            get => GetValue(DialogContentProperty);
            set => SetValue(DialogContentProperty, value);
        }

        public DialogPanel()
        {
            InitializeComponent();
        }
    }
}
