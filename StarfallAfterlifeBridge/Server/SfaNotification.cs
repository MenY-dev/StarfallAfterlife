using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public class SfaNotification
    {
        public string Id { get; set; }

        public string Header { get; set; }

        public string Text { get; set; }

        public SfaNotificationType Type { get; set; } = SfaNotificationType.Info;

        public float LifeTime { get; set; } = 5;

        public Dictionary<string, string> Format { get; set; }
    }
}
