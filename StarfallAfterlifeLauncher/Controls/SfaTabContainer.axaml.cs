using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class SfaTabContainer : Panel
    {
        public static readonly DirectProperty<SfaTabContainer, SfaTabPageContainer> PageContainerProperty =
            AvaloniaProperty.RegisterDirect<SfaTabContainer, SfaTabPageContainer>(
                nameof(PageContainer),
                o => o.PageContainer,
                (o, v) => o.PageContainer = v,
                unsetValue: null,
                defaultBindingMode: BindingMode.TwoWay);

        public SfaTabPageContainer PageContainer
        {
            get => pageContainer;
            set => SetAndRaise(PageContainerProperty, ref pageContainer, value);
        }
        
        private SfaTabPageContainer pageContainer;
        
        public void SetSelection(SfaTab tab, bool selected)
        {
            if (selected == true)
            {
                foreach (var item in GetChildTabs())
                {
                    var newState = item == tab;

                    if (item.IsSelected != newState)
                        item.IsSelected = newState;
                }
            }

            if (selected == false || tab == null)
                PageContainer?.ShowTab(null);
            else
                PageContainer?.ShowTab(tab.PageName);
        }

        public List<SfaTab> GetChildTabs()
        {
            List<SfaTab> result = new();
            Queue<Visual> queue = new(new Visual[]{ this });

            do
            {
                var visual = queue.Dequeue();

                if (visual is null)
                    continue;

                foreach (Visual item in
                    visual.GetVisualChildren() ??
                    Enumerable.Empty<Visual>())
                    queue.Enqueue(item);

                if (visual is SfaTab tab)
                    result.Add(tab);

            } while (queue.Count > 0);

            return result;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == PageContainerProperty)
            {
                var currentTab = GetChildTabs().FirstOrDefault(t => t.IsSelected == true);
                (change.NewValue as SfaTabPageContainer)?.ShowTab(currentTab?.PageName);
            }
        }
    }
}
