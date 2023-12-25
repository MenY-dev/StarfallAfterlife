using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class SFAWaitingPopup : SfaPopup
    {
        protected override Type StyleKeyOverride => typeof(SfaPopup);

        public SFAWaitingPopup()
        {
            InitializeComponent();
        }
    }
}
