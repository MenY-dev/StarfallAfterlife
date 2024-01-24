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
using StarfallAfterlife.Bridge.Serialization;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class EditNamePopup : SfaPopup, INotifyDataErrorInfo
    {
        protected override Type StyleKeyOverride => typeof(EditNamePopup);

        public static readonly StyledProperty<bool> IsValidProperty = AvaloniaProperty.Register<EditNamePopup, bool>(
            nameof(IsValid), false, true, BindingMode.OneWay);

        public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<EditNamePopup, string>(
            nameof(Label), null, true, BindingMode.TwoWay);

        public static readonly StyledProperty<string> TextFilterProperty = AvaloniaProperty.Register<EditNamePopup, string>(
            nameof(TextFilter), @"[A-Za-z0-9\-_]*$", true, BindingMode.TwoWay);

        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<EditNamePopup, string>(
            nameof(Text), defaultBindingMode: BindingMode.TwoWay);

        public string TextFilter
        {
            get => GetValue(TextFilterProperty);
            set => SetValue(TextFilterProperty, value);
        }

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public bool IsValid
        {
            get => GetValue(IsValidProperty);
            set => SetValue(IsValidProperty, value);
        }

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public bool IsDone { get; protected set; }

        public bool HasErrors { get; protected set; }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private string _text;

        public void OkPressed()
        {
            IsDone = true;
            Close();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TextProperty)
            {
                var value = change.NewValue as string;

                try
                {
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
                }
                catch
                {
                    IsValid = false;
                    HasErrors = false;
                }

                ErrorsChanged?.Invoke(this, new(nameof(Text)));
            }
        }

        public Task<EditNamePopup> ShowDialog(string defaultName = null)
        {
            Text = defaultName;
            return base.ShowDialog().ContinueWith(t => this);
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (HasErrors == true)
                yield return App.GetString("s_dialog_edit_name_error") ?? "Invalid symbols!";
        }
    }
}
