using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
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

        [JsonPropertyName("ranked_fleets")]
        public List<RankedFleetInfo> RankedFleets { get; set; } = new(); 

        [JsonPropertyName("discovery_mode_profile")]
        public DiscoveryProfile DiscoveryModeProfile { get; set; } = new DiscoveryProfile();


        public event EventHandler<EventArgs> Edited;

        [JsonIgnore]
        public int BM { get; set; } = 1;

        [JsonIgnore]
        public int SFC { get; set; } = 10;

        [JsonIgnore]
        public int Ban { get; set; } = 0;

        [JsonIgnore]
        public int CharacterSlotlimit { get; set; } = 5;

        [JsonIgnore]
        public int DropShipProgressionSFC { get; set; } = 200;

        [JsonIgnore]
        public int DropShipProgressionIGC { get; set; } = 20000;

        [JsonIgnore]
        public int ProductionPointsCost60SFC { get; set; } = 100;

        [JsonIgnore]
        public int ProductionPointsCost60IGC { get; set; } = 2500;

        [JsonIgnore]
        public int RushOpenWeeklyReward { get; set; } = 100;

        [JsonIgnore]
        public Character CurrentCharacter { get; set; } = null;

        [JsonIgnore]
        public SfaDatabase Database { get; set; } = null;

        [JsonIgnore]
        public WeeklyQuestsInfo Seasons { get; set; } = new();

        [JsonIgnore]
        public List<BGShopItem> BGShop { get; set; } = new();

        [JsonIgnore]
        public int UniqueId { get; internal set; } = 0;

        [JsonIgnore]
        public string UniqueName { get; internal set; }

        [JsonIgnore]
        public int IndexSpace { get; internal set; } = 0;

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
