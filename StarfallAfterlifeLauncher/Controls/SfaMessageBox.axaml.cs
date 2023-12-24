using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using StarfallAfterlife.Launcher.Views;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class SfaMessageBox : Window
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<SfaMessageBox, string>(
                name: nameof(Text),
                defaultValue: null,
                defaultBindingMode: BindingMode.TwoWay);


        public static readonly StyledProperty<MessageBoxButton> ButtonsProperty =
            AvaloniaProperty.Register<SfaMessageBox, MessageBoxButton>(
                name: nameof(Buttons),
                defaultValue: MessageBoxButton.Ok,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<MessageBoxButton> PressedButtonProperty =
            AvaloniaProperty.Register<SfaMessageBox, MessageBoxButton>(
                name: nameof(PressedButton),
                defaultValue: MessageBoxButton.Undefined,
                defaultBindingMode: BindingMode.OneWay);

        public string Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }

        public MessageBoxButton Buttons { get => GetValue(ButtonsProperty); set { SetValue(ButtonsProperty, value); UpdateButtons(); } }

        public MessageBoxButton PressedButton { get => GetValue(PressedButtonProperty); set { SetValue(PressedButtonProperty, value); } }

        public SfaMessageBox()
        {
            InitializeComponent();
            UpdateButtons();
        }

        public void OnButtonClick(object button)
        {
            PressedButton = (button as MessageBoxButton?) ?? MessageBoxButton.Undefined;
            Close();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        public void UpdateButtons()
        {
            if (this.FindControl<StackPanel>("buttons") is StackPanel root &&
                root.Children is AvaloniaList<Control> controls &&
                controls.Count > 0)
            {
                foreach (var item in controls)
                {
                    if (item is not null &&
                        (item as Button)?.CommandParameter is MessageBoxButton button)
                    {
                        item.IsVisible = Buttons.HasFlag(button);
                    }
                }
            }
        }

        public static Task<MessageBoxButton> ShowDialog(string message, string title = null, MessageBoxButton buttons = MessageBoxButton.Ok)
        {
            var root = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as MainWindow;

            var dialog = new SfaMessageBox
            {
                Text = message,
                Title = title,
                Buttons = buttons,
            };

            if (root is null)
                return Task.FromResult(MessageBoxButton.Undefined);

            return dialog.ShowDialog(root).ContinueWith(t => Dispatcher.UIThread.Invoke(() => dialog.PressedButton));
        }
    }
}
