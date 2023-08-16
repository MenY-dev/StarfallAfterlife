using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public partial class SfaGameProfile
    {
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = "NewPlayer";

        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.Empty;

        [JsonPropertyName("temporarypass")]
        public string TemporaryPass { get; set; } = "a0b1c2d3e4f5g6h7i8j9k10l";

        [JsonPropertyName("ranked_mode_profile")]
        public RankedProfile RankedModeProfile { get; set; } = new RankedProfile();

        [JsonPropertyName("discovery_mode_profile")]
        public DiscoveryProfile DiscoveryModeProfile { get; set; } = new DiscoveryProfile();

        public event EventHandler<EventArgs> Edited;

        [JsonIgnore]
        public Character CurrentCharacter { get; set; } = null;

        [JsonIgnore]
        public SfaDatabase Database { get; set; } = null;

        [JsonIgnore]
        protected object Locker { get; } = new object();

        public void Use(Action<UsageHandler> action)
        {
            lock (Locker)
            {
                UsageHandler handler = new UsageHandler(this);
                action.Invoke(handler);

                if (handler.Edited == true)
                    OnEdited(EventArgs.Empty);
            }
        }

        protected virtual void OnEdited(EventArgs args)
        {
            Edited?.Invoke(this, args);
        }

        public class UsageHandler
        {
            public SfaGameProfile Profile { get; }

            public bool Edited
            {
                get => edited;
                set
                {
                    if (value == true && value != edited)
                        edited = value;
                }
            }

            private bool edited = false;

            public UsageHandler(SfaGameProfile profile)
            {
                Profile = profile;
            }
        }
    }
}
