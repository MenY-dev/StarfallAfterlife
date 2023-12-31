using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Styling;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace StarfallAfterlife.Launcher.Controls
{
    public class SidebarTab : UserControl
    {
        public static readonly DirectProperty<SidebarTab, string> PageNameProperty =
            AvaloniaProperty.RegisterDirect<SidebarTab, string>(
                nameof(PageName),
                o => o.PageName,
                (o, v) => o.PageName = v,
                unsetValue: null,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly DirectProperty<SidebarTab, bool> IsSelectedProperty =
            AvaloniaProperty.RegisterDirect<SidebarTab, bool>(
                nameof(IsSelected),
                o => o.IsSelected,
                (o, v) => o.IsSelected = v,
                unsetValue: false,
                defaultBindingMode: BindingMode.TwoWay);

        public string PageName
        {
            get => pageName;
            set => SetAndRaise(PageNameProperty, ref pageName, value);
        }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                SetAndRaise(IsSelectedProperty, ref isSelected, value);
            }
        }

        private string pageName;
        private bool isSelected;

        protected ToggleButton Toggle { get; set; }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change?.Property == IsSelectedProperty &&
                (bool)change.OldValue != (bool)change.NewValue)
            {
                SetSelection(change.NewValue as bool? ?? false);
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            Toggle = e.NameScope.Find<ToggleButton>("InnerToggle");
        }

        private void OnToggleClick(object sender, RoutedEventArgs e)
        {
            IsSelected = Toggle?.IsChecked ?? false;
        }

        public virtual void SetSelection(bool selected)
        {
            GetParentTabContainer()?.SetSelection(this, selected);
        }

        private Sidebar GetParentTabContainer()
        {
            var parent = Parent;

            while (parent is not null)
            {
                if (parent is Sidebar container)
                    return container;
                
                parent = parent.Parent;
            }

            return null;
        }
    }
}
