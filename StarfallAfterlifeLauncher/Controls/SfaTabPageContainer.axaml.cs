using Avalonia.Controls;
using Avalonia.Styling;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class SfaTabPageContainer : Panel
    {
        protected string CurentTabName { get; set; }

        public void ShowTab(string tabName)
        {
            CurentTabName = tabName;

            if (this.Children is not null)
            {
                foreach (var child in Children)
                {
                    if (child is SfaTabPage page)
                        page.IsShow = page.Name == tabName;
                }
            }
        }

        protected override void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.ChildrenChanged(sender, e);

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is SfaTabPage page)
                        page.IsShow =
                            page.Name != null &&
                            page.Name == CurentTabName;
                }
            }
        }

        public override void EndInit()
        {
            base.EndInit();
            ShowTab(CurentTabName);
        }
    }
}
