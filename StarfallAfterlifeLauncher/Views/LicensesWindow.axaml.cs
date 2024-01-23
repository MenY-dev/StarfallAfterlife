using Avalonia;
using Avalonia.Controls;
using StarfallAfterlife.Bridge.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace StarfallAfterlife.Launcher.Views
{
    public partial class LicensesWindow : Window
    {
        public record LicenseInfo(string Name, string Text);

        public static readonly StyledProperty<LicenseInfo> SelectedLicenseProperty =
            AvaloniaProperty.Register<LicensesWindow, LicenseInfo>(nameof(SelectedLicense));

        public ObservableCollection<LicenseInfo> Licenses { get; } = new();

        public LicenseInfo SelectedLicense
        {
            get => GetValue(SelectedLicenseProperty);
            set => SetValue(SelectedLicenseProperty, value);
        }

        public LicensesWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            LoadLicenses();
        }

        public void LoadLicenses()
        {
            try
            {
                var currentLicenseName = SelectedLicense?.Name;

                Licenses.Clear();
                SelectedLicense = null;

                var dir = Path.Combine("", "Licenses");

                if (Directory.Exists(dir) &&
                    FileHelpers.GetDirectoriesSelf(dir) is string[] dirs)
                {
                    foreach (var licenseDir in dirs)
                    {
                        try
                        {
                            var licenseFile = Path.Combine(licenseDir, "License.txt");

                            if (File.Exists(licenseFile) == true)
                                Licenses.Add(new(
                                    Path.GetFileName(licenseDir),
                                    File.ReadAllText(licenseFile)));
                        }
                        catch { }
                    }
                }

                SelectedLicense =
                    Licenses.FirstOrDefault(l => l?.Name == currentLicenseName) ??
                    Licenses.FirstOrDefault();

            }
            catch
            {
                Licenses.Clear();
                SelectedLicense = null;
            }
        }
    }
}
