using Avalonia;
using Avalonia.Controls;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class LogMsg : UserControl
    {
        public static readonly StyledProperty<bool> IsExpandedProperty =
            AvaloniaProperty.Register<LogMsg, bool>(nameof(IsExpanded), false);

        public static readonly StyledProperty<bool> NeedCollapseProperty =
            AvaloniaProperty.Register<LogMsg, bool>(nameof(NeedCollapse), false);

        public static readonly StyledProperty<double> CollapsedHeightProperty =
            AvaloniaProperty.Register<LogMsg, double>(nameof(CollapsedHeight), 100);

        public bool IsExpanded
        {
            get => GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public bool NeedCollapse
        {
            get => GetValue(NeedCollapseProperty);
            set => SetValue(NeedCollapseProperty, value);
        }

        public double CollapsedHeight
        {
            get => GetValue(CollapsedHeightProperty);
            set => SetValue(CollapsedHeightProperty, value);
        }

        public LogMsg()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == DataContextProperty ||
                change.Property == IsExpandedProperty ||
                change.Property == CollapsedHeightProperty ||
                change.Property == NeedCollapseProperty)
            {
                if (NeedCollapse == true &&
                    IsExpanded == false)
                {
                    MaxHeight = CollapsedHeight;
                }
                else
                {
                    ClearValue(MaxHeightProperty);
                }
            }
        }
    }
}
