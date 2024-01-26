using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StarfallAfterlife.Launcher.Controls
{
    public class PointerPressedBehav : AvaloniaObject
    {
        public static readonly AttachedProperty<ICommand> CommandProperty = AvaloniaProperty.RegisterAttached<PointerPressedBehav, Interactive, ICommand>(
            "Command", default, false, BindingMode.OneTime);

        static PointerPressedBehav()
        {
            CommandProperty.Changed.AddClassHandler<Interactive>(HandleCommandChanged);
        }

        private static void HandleCommandChanged(Interactive interactElem, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.NewValue is ICommand commandValue)
            {
                interactElem.AddHandler(InputElement.PointerPressedEvent, Handler);
            }
            else
            {
                interactElem.RemoveHandler(InputElement.PointerPressedEvent, Handler);
            }

            static void Handler(object s, RoutedEventArgs e)
            {
                if (s is Interactive interactElem)
                {
                    ICommand commandValue = interactElem.GetValue(CommandProperty);

                    if (commandValue?.CanExecute(e) == true)
                    {
                        commandValue.Execute(e);
                    }
                }
            }
        }

        public static void SetCommand(AvaloniaObject element, ICommand commandValue) =>
            element.SetValue(CommandProperty, commandValue);

        public static ICommand GetCommand(AvaloniaObject element) =>
            element.GetValue(CommandProperty);
    }
}
