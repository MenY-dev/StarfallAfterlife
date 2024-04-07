using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class SfaTreeViewItem : TreeViewItem
    {
        protected override Type StyleKeyOverride => typeof(TreeViewItem);
        private Control _headerPresenter;

        protected override void OnHeaderDoubleTapped(TappedEventArgs e)
        {
            //base.OnHeaderDoubleTapped(e);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (_headerPresenter is InputElement currentElement)
                currentElement.Tapped -= OnHeaderTapped;
            
            _headerPresenter = e.NameScope.Find<Control>("PART_HeaderPresenter");

            if (_headerPresenter is InputElement element)
                element.Tapped += OnHeaderTapped;
        }

        protected virtual void OnHeaderTapped(object sender, TappedEventArgs e)
        {
            if (ItemCount > 0)
            {
                SetCurrentValue(IsExpandedProperty, !IsExpanded);
                e.Handled = true;
            }
        }
    }
}
