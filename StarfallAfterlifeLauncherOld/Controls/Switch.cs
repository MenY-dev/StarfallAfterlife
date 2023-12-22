using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class Switch : Panel
    {
        public static readonly DirectProperty<Switch, object> ValueProperty =
            AvaloniaProperty.RegisterDirect<Switch, object>(
                nameof(Value),
                o => o.Value,
                (o, v) => o.Value = v,
                unsetValue: null,
                defaultBindingMode: BindingMode.OneWay);

        public static readonly AttachedProperty<object> CaseProperty =
            AvaloniaProperty.RegisterAttached<Switch, Interactive, object>(
                "Case", null, false, BindingMode.OneWay);

        public static readonly AttachedProperty<bool> DefaultProperty =
            AvaloniaProperty.RegisterAttached<Switch, Interactive, bool>(
                "Default", false, false, BindingMode.OneWay);

        public object Value
        {
            get => targetValue;
            set
            {
                SetAndRaise(ValueProperty, ref targetValue, value);
                UpdateAllCases();
            }
        }

        private object targetValue;

        protected virtual void UpdateAllCases()
        {
            if (Children is null)
                return;

            var caseFound = false;

            foreach (var child in Children)
            {
                if (child is not null)
                {
                    var caseValue = GetCase(child);
                    var isVisible = caseValue?.Equals(Value) ?? Value == null;
                    caseFound |= isVisible;
                    child.IsVisible = isVisible;
                }
            }

            if (caseFound == true)
                return;

            foreach (var child in Children)
            {
                if (child is not null)
                {
                    var defaultValue = GetDefault(child);
                    child.IsVisible = defaultValue == true;
                }
            }
        }

        public static void SetCase(AvaloniaObject element, object parameter)
        {
            element.SetValue(CaseProperty, parameter);

            if (element is Control control &&
                control.Parent is Switch parent)
                parent.UpdateAllCases();
        }

        public static object GetCase(AvaloniaObject element)
        {
            return element.GetValue(CaseProperty);
        }

        public static void SetDefault(AvaloniaObject element, bool parameter)
        {
            element.SetValue(DefaultProperty, parameter);

            if (element is Control control &&
                control.Parent is Switch parent)
                parent.UpdateAllCases();
        }

        public static bool GetDefault(AvaloniaObject element)
        {
            return element.GetValue(DefaultProperty);
        }
    }
}
