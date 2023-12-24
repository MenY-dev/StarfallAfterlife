using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace StarfallAfterlife.Launcher.Controls
{
    [PseudoClasses(":expanded", ":collapsed", ":relaxed")]
    public class Sidebar : UserControl
    {
        public static readonly StyledProperty<SidebarPageContainer> PageContainerProperty =
            AvaloniaProperty.Register<Sidebar, SidebarPageContainer>(
                name: nameof(PageContainer),
                defaultValue: null,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<bool> IsExpandedProperty =
            AvaloniaProperty.Register<Sidebar, bool>(
                name: nameof(IsExpanded),
                defaultValue: false,
                defaultBindingMode: BindingMode.TwoWay);
        public SidebarPageContainer PageContainer
        {
            get => GetValue(PageContainerProperty);
            set => SetValue(PageContainerProperty, value);
        }

        public bool IsExpanded
        {
            get => GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public void SetSelection(SidebarTab tab, bool selected)
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
            {
                PageContainer?.ShowTab(null);
            }
            else
            {
                PageContainer?.ShowTab(tab.PageName);
            }

            IsExpanded = false;
        }

        public List<SidebarTab> GetChildTabs()
        {
            List<SidebarTab> result = new();
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

                if (visual is SidebarTab tab)
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
                (change.NewValue as SidebarPageContainer)?.ShowTab(currentTab?.PageName);
            }
            else if (change.Property == IsExpandedProperty)
            {
                var value = (bool?)change.NewValue ?? false;
                PseudoClasses.Set(":expanded", value == true);
                PseudoClasses.Set(":collapsed", value == false);
            }
        }

        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            IsExpanded = true;
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            IsExpanded = false;
        }
    }
}
