using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Styling;
using StarfallAfterlife.Launcher.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class EditNamePopup : Window, INotifyDataErrorInfo
    {
        protected override Type StyleKeyOverride => typeof(EditNamePopup);

        public static AvaloniaProperty IsValidProperty = AvaloniaProperty.Register<CreateProfilePopup, bool>(
            nameof(IsValid), false, true, BindingMode.TwoWay);

        public static AvaloniaProperty LabelProperty = AvaloniaProperty.Register<CreateProfilePopup, string>(
            nameof(Label), null, true, BindingMode.TwoWay);

        public bool IsDone { get; protected set; }

        public string Text
        {
            get => _text;
            protected set
            {
                _text = value;

                if (string.IsNullOrEmpty(value) == true)
                {
                    IsValid = false;
                    HasErrors = false;
                }
                else if (new Regex(@"[A-Za-z0-9\-_]*$").Matches(value) is MatchCollection matches &&
                    matches.FirstOrDefault(m => m.Index == 0 && m.Length == value.Length) is null)
                {
                    IsValid = false;
                    HasErrors = true;
                }
                else
                {
                    IsValid = true;
                    HasErrors = false;
                }

                ErrorsChanged?.Invoke(this, new(nameof(Text)));
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool IsValid { get => (bool)GetValue(IsValidProperty); set => SetValue(IsValidProperty, value); }

        public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

        public bool HasErrors { get; protected set; }

        private string _text;

        public void OkPressed()
        {
            IsDone = true;
            Close();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
        }

        public Task<EditNamePopup> ShowDialog(string defaultName = null)
        {
            var root = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as MainWindow;

            Text = defaultName;

            if (root is null)
                return Task.FromResult(this);

            return ShowDialog(root).ContinueWith(t => this);
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (HasErrors == true)
                yield return "Invalid symbols!";
        }
    }
}
