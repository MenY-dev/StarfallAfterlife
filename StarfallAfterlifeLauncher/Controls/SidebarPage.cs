using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Data;
using Avalonia.Metadata;

namespace StarfallAfterlife.Launcher.Controls
{
    [PseudoClasses(":pageshow", ":pagehide")]
    public class SidebarPage : UserControl
    {
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
            }
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
