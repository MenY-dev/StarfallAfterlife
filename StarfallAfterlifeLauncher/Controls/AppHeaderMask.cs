using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class AppHeaderMask : Panel
    {
        public static readonly StyledProperty<AppHeader> HeaderProperty =
            AvaloniaProperty.Register<Sidebar, AppHeader>(
                name: nameof(HeaderProperty),
                defaultValue: null,
                defaultBindingMode: BindingMode.TwoWay);

        public AppHeader Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public virtual void UpdateMask()
        {
            Clip = Header?.CreateMaskGeometry(this);
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateMask();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == HeaderProperty)
            {
                UpdateMask();
            }
        }
    }
}
