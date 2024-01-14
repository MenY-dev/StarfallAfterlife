using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class CreateRealmPopup : SfaPopup, INotifyDataErrorInfo
    {
        public static readonly StyledProperty<string> RealmNameProperty =
            AvaloniaProperty.Register<EditNamePopup, string>(nameof(RealmName), "NewRealm");

        public static readonly StyledProperty<string> RealmDescriptionProperty =
            AvaloniaProperty.Register<EditNamePopup, string>(nameof(RealmDescription));

        public static readonly StyledProperty<int> RealmSeedProperty =
            AvaloniaProperty.Register<EditNamePopup, int>(nameof(RealmSeed), 1);

        public static readonly StyledProperty<bool> IsValidProperty = AvaloniaProperty.Register<EditNamePopup, bool>(
            nameof(IsValid), false, true, BindingMode.TwoWay);

        public static readonly StyledProperty<bool> EditRealmProperty = AvaloniaProperty.Register<EditNamePopup, bool>(
            nameof(EditRealm), false, true, BindingMode.TwoWay);

        public string RealmName
        {
            get => GetValue(RealmNameProperty);
            set
            {
                if (string.IsNullOrEmpty(value) == true)
                {
                    IsValid = false;
                    HasErrors = false;
                }
                else if (new Regex(_namePattern).Matches(value) is MatchCollection matches &&
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
                SetValue(RealmNameProperty, value);
            }
        }

        public string RealmDescription
        {
            get => GetValue(RealmDescriptionProperty);
            set => SetValue(RealmDescriptionProperty, value);
        }

        public int RealmSeed
        {
            get => GetValue(RealmSeedProperty);
            set => SetValue(RealmSeedProperty, value);
        }

        public bool IsValid
        {
            get => GetValue(IsValidProperty);
            set => SetValue(IsValidProperty, value);
        }

        public bool EditRealm
        {
            get => GetValue(EditRealmProperty);
            set => SetValue(EditRealmProperty, value);
        }

        public bool IsDone { get; protected set; } = false;

        public bool HasErrors { get; protected set; } = false;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private const string _namePattern = @"[A-Za-z0-9\-_]+([ ]*[A-Za-z0-9\-_])*$";

        public CreateRealmPopup()
        {
            DataContext = this;
            InitializeComponent();
        }

        public void OkPressed()
        {
            IsDone = true;
            Close();
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (HasErrors == true)
                yield return App.GetString("s_dialog_edit_name_error") ?? "Invalid symbols!";
        }
    }
}
