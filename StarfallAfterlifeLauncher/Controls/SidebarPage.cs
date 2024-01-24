using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Data;
using Avalonia.Metadata;
using System.Windows.Input;

namespace StarfallAfterlife.Launcher.Controls
{
    [PseudoClasses(":pageshow", ":pagehide")]
    public class SidebarPage : UserControl
    {
        public static readonly StyledProperty<ICommand> ShowCommandProperty =
            AvaloniaProperty.Register<SidebarPage, ICommand>(nameof(ShowCommand));

        public static readonly DirectProperty<SidebarPage, bool> IsShowProperty =
            AvaloniaProperty.RegisterDirect<SidebarPage, bool>(
                nameof(IsShow),
                o => o.IsShow,
                (o, v) => o.IsShow = v,
                unsetValue: false,
                defaultBindingMode: BindingMode.TwoWay);

        public bool IsShow
        {
            get => isShow;
            set
            {
                SetAndRaise(IsShowProperty, ref isShow, value);
                ApplyPseudoClasses(IsShow);

                if (value == true)
                    ShowCommand?.Execute(null);
            }
        }

        public ICommand ShowCommand
        {
            get => GetValue(ShowCommandProperty);
            set => SetValue(ShowCommandProperty, value);
        }

        private bool isShow = true;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ApplyPseudoClasses(IsShow);
        }

        protected void ApplyPseudoClasses(bool isShow)
        {
            PseudoClasses.Set(":pageshow", isShow == true);
            PseudoClasses.Set(":pagehide", isShow == false);
        }
    }
}
