using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Styling;
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class CreateProfilePopup : EditNamePopup
    {
        protected override Type StyleKeyOverride => typeof(EditNamePopup);

        public CreateProfilePopup()
        {
            InitializeComponent();
        }
    }
}
