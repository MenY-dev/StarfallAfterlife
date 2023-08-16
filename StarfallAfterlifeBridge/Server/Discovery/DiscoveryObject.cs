using StarfallAfterlife.Bridge.Events;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization.Json;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class DiscoveryObject : SfaObject, IGalaxyMapObject
    {

        public int Id { get; set; } = -1;

        public virtual DiscoveryObjectType Type { get; set; } = DiscoveryObjectType.None;

        public virtual Vector2 Location { get; set; } = Vector2.Zero;

        public DiscoveryGalaxy Galaxy { get; set; }

        public DiscoveryObject ParentObject
        {
            get => parentObject;
            set
            {
                if (value != parentObject)
                {
                    var oldParent = parentObject;
                    parentObject = value;
                    OnParentObjectChanged(parentObject, oldParent);
                }
            }
        }

        public MulticastEvent Listeners { get; } = new();

        GalaxyMapObjectType IGalaxyMapObject.ObjectType => (GalaxyMapObjectType)Type;

        int IGalaxyMapObject.Id { get => Id; set { } }

        int IGalaxyMapObject.X => SystemHexMap.SystemPointToHex(Location).X;

        int IGalaxyMapObject.Y => SystemHexMap.SystemPointToHex(Location).Y;

        protected DiscoveryObject parentObject = null;

        protected virtual void OnParentObjectChanged(DiscoveryObject newParent, DiscoveryObject oldParent) { }

        public override JsonNode ToJson()
        {
            var doc = base.ToJson() ?? new JsonObject();

            doc["id"] = Id;
            doc["type"] = (byte)Type;
            doc["x"] = Location.X;
            doc["y"] = Location.Y;

            return doc;
        }

        public override void LoadFromJson(JsonNode doc)
        {
            base.LoadFromJson(doc);

            if (doc is null)
                return;

            Id = (int?)doc["id"] ?? -1;
            Type = (DiscoveryObjectType?)(byte?)doc["type"] ?? DiscoveryObjectType.None;
            Location = new Vector2(
                (float?)doc["x"] ?? 0,
                (float?)doc["y"] ?? 0);
        }

        public virtual void AddListener(IDiscoveryListener listener) => Listeners.Add(listener);

        public virtual void RemoveListener(IDiscoveryListener listener) => Listeners.Remove(listener);

        public virtual bool ContainsListener(IDiscoveryListener listener) => Listeners.Contains(listener);

        public virtual void Broadcast<TListener>(Action<TListener> action) where TListener : IDiscoveryListener => 
            Listeners.Broadcast(action);

        public virtual void Broadcast<TListener>(Action<TListener> action, Func<TListener, bool> predicate) where TListener : IDiscoveryListener =>
            Listeners.Broadcast(action, predicate);
    }
}
