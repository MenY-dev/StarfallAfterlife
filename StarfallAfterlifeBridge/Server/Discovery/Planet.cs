using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using static StarfallAfterlife.Bridge.Native.Windows.Win32;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class Planet : DockableObject
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.Planet;

        public string Name { get; set; }

        public PlanetType PlanetType { get; set; }

        public int Size { get; set; }

        public float Temperature { get; set; }

        public float Atmosphere { get; set; }

        public float Gravitation { get; set; }

        public float NoubleGases { get; set; }

        public float RadiactiveMetals { get; set; }

        public float SuperConductors { get; set; }

        public int Circle { get; set; }

        public List<int> SecretLocations { get; set; } = new List<int>() { 0 };

        public Planet() { }

        public Planet(int id)
        {
            Id = id;
        }


        public override void Init()
        {
            base.Init();

            if (System?.Info is GalaxyMapStarSystem system &&
                system.Planets.FirstOrDefault(i => i.Id == Id) is GalaxyMapPlanet info)
            {
                Name = info.Name;
                Faction = info.Faction;
                PlanetType = info.Type;
                Size = info.Size;
                Hex = info.Hex;
                Temperature = info.Temperature;
                Atmosphere = info.Atmosphere;
                Gravitation = info.Gravitation;
                NoubleGases = info.NoubleGases;
                RadiactiveMetals = info.RadiactiveMetals;
                SuperConductors = info.SuperConductors;
                SecretLocations = info.SecretLocations;
                Circle = system?.Level ?? -1;
            }
        }
    }
}
