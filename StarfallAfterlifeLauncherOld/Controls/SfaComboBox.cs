using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class SfaComboBox : ComboBox
    {
        protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.OnTemplateChanged(e);
        }

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new SfaComboBoxItem();
        }
    }
}
