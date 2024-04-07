using Avalonia;
using Avalonia.Controls;
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
    public class ShiftPressedBehav : AvaloniaObject
    {
        public static readonly AttachedProperty<ICommand> DownCommandProperty =
            AvaloniaProperty.RegisterAttached<DataGridColumnsBehav, Interactive, ICommand>(
            "DownCommand", default, false);

        public static readonly AttachedProperty<ICommand> UpCommandProperty =
            AvaloniaProperty.RegisterAttached<DataGridColumnsBehav, Interactive, ICommand>(
            "UpCommand", default, false);

        protected static readonly AttachedProperty<bool> StateProperty =
            AvaloniaProperty.RegisterAttached<DataGridColumnsBehav, Interactive, bool>(
            "State", default, false);

        static ShiftPressedBehav()
        {
            DownCommandProperty.Changed.AddClassHandler<Interactive>(HandleStatePropertyChanged);
            UpCommandProperty.Changed.AddClassHandler<Interactive>(HandleStatePropertyChanged);
        }

        private static void HandleStatePropertyChanged(Interactive interactive, AvaloniaPropertyChangedEventArgs args)
        {
            if (interactive is null)
                return;

            if (args.Property == DownCommandProperty)
            {
                interactive.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
            }
            else if (args.Property == UpCommandProperty)
            {
                interactive.AddHandler(InputElement.KeyUpEvent, OnKeyUp, RoutingStrategies.Tunnel);
                interactive.AddHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost, RoutingStrategies.Direct);
                interactive.AddHandler(InputElement.LostFocusEvent, OnLostFocus, RoutingStrategies.Direct);
            }
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is Interactive interactive &&
                (e.KeyModifiers.HasFlag(KeyModifiers.Shift) ||
                e.KeyModifiers.HasFlag(KeyModifiers.Control)))
            {
                SetState(interactive, true);
                GetDownCommand(interactive)?.Execute(true);
            }
        }

        private static void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (sender is Interactive interactive)
            {
                if (GetState(interactive) == false)
                    return;

                SetState(interactive, false);
                GetUpCommand(interactive)?.Execute(false);
            }
        }

        private static void LostFocuse(object sender, KeyEventArgs e)
        {
            if (sender is Interactive interactive)
            {
                if (GetState(interactive) == false)
                    return;

                SetState(interactive, false);
                GetUpCommand(interactive)?.Execute(false);
            }
        }

        private static void OnPointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
        {
            if (sender is Interactive interactive)
            {
                if (GetState(interactive) == false)
                    return;

                SetState(interactive, false);
                GetUpCommand(interactive)?.Execute(false);
            }
        }

        private static void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Interactive interactive)
            {
                if (GetState(interactive) == false)
                    return;

                SetState(interactive, false);
                GetUpCommand(interactive)?.Execute(false);
            }
        }

        public static void SetDownCommand(AvaloniaObject element, ICommand command) =>
            element.SetValue(DownCommandProperty, command);

        public static ICommand GetDownCommand(AvaloniaObject element) =>
            element.GetValue(DownCommandProperty);

        public static void SetUpCommand(AvaloniaObject element, ICommand command) =>
            element.SetValue(UpCommandProperty, command);

        public static ICommand GetUpCommand(AvaloniaObject element) =>
            element.GetValue(UpCommandProperty);

        public static void SetState(AvaloniaObject element, bool state) =>
            element.SetValue(StateProperty, state);

        public static bool GetState(AvaloniaObject element) =>
            element.GetValue(StateProperty);
    }
}
