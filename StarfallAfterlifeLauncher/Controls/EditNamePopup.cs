using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Specialized;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class EditNamePopup : SfaPopup, INotifyDataErrorInfo
    {
        protected override Type StyleKeyOverride => typeof(EditNamePopup);

        public static readonly StyledProperty<bool> IsValidProperty = AvaloniaProperty.Register<EditNamePopup, bool>(
            nameof(IsValid), false, true, BindingMode.TwoWay);

        public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<EditNamePopup, string>(
            nameof(Label), null, true, BindingMode.TwoWay);

        public static readonly StyledProperty<string> TextFilterProperty = AvaloniaProperty.Register<EditNamePopup, string>(
            nameof(Label), @"[A-Za-z0-9\-_]*$", true, BindingMode.TwoWay);

        public bool IsDone { get; protected set; }

        public string TextFilter
        {
            get => GetValue(TextFilterProperty);
            set => SetValue(TextFilterProperty, value);
        }

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
                else if (new Regex(TextFilter).Matches(value) is MatchCollection matches &&
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
            Text = defaultName;
            return base.ShowDialog().ContinueWith(t => this);
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (HasErrors == true)
                yield return "Invalid symbols!";
        }
    }
}
