using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class SfaTreeView : TreeView
    {
        protected override Type StyleKeyOverride => typeof(TreeView);

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new SfaTreeViewItem();
        }
    }
}
