using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServer
    {
        public bool RenameSystem(int id, string newName, string author)
        {
            RealmObjectRenameInfo info = null;

            RealmInfo?.Use(r =>
            {
                var variableMap = r.Realm.Variable ??= new();
                var systems = variableMap.RenamedSystems ??= new();

                if (systems.ContainsKey(id) == false)
                {
                    systems[id] = info = new RealmObjectRenameInfo() { Id = id, Name = newName, Char = author };
                    r.SaveVariable();
                }
                else if (newName is null)
                {
                    info = new RealmObjectRenameInfo() { Id = id, Name = null, Char = null };
                    systems.Remove(id);
                }
            });

            if (info is not null)
            {
                UseClients(clients =>
                {
                    foreach (var client in clients)
                    {
                        if (client is null ||
                            client.IsConnected == false ||
                            client.IsPlayer == false)
                            continue;

                        client?.DiscoveryClient?.SendStarRenamed(info.Id, info.Name, info.Char);
                    }
                });

                SyncVariableMap(renamedSystems: new[] { info });
            }

            return info is not null;
        }

        public bool RenamePlanet(int id, string newName, string author)
        {
            RealmObjectRenameInfo info = null;

            RealmInfo?.Use(r =>
            {
                var variableMap = r.Realm.Variable ??= new();
                var planets = variableMap.RenamedPlanets ??= new();

                if (planets.ContainsKey(id) == false)
                {
                    planets[id] = info = new RealmObjectRenameInfo() { Id = id, Name = newName, Char = author };
                    r.SaveVariable();
                }
                else if (newName is null)
                {
                    info = new RealmObjectRenameInfo() { Id = id, Name = null, Char = null };
                    planets.Remove(id);
                }
            });

            if (info is not null &&
                Galaxy?.Map?.GetSystem(GalaxyMapObjectType.Planet, id)?.Id is int systemId &&
                Galaxy.GetActiveSystem(systemId, false) is StarSystem system)
            {
                UseClients(clients =>
                {
                    foreach (var client in clients)
                    {
                        if (client is not null &&
                            client.IsConnected == true &&
                            client.IsPlayer == true &&
                            client.DiscoveryClient is DiscoveryClient discoveryClient &&
                            discoveryClient.CurrentCharacter?.Fleet?.System?.Id == systemId)
                        {
                            discoveryClient.RequestDiscoveryObjectSync(systemId, DiscoveryObjectType.Planet, id);
                            discoveryClient.SyncDiscoveryObject(systemId, DiscoveryObjectType.Planet, id);
                        }
                    }
                });

                SyncVariableMap(renamedPlanets: new[] { info });
            }

            return info is not null;
        }

        public void ReportSystemName(int id, SfaServerClient author)
        {
            if (author is null)
                return;

            RealmInfo?.Use(r =>
            {
                if (r.Realm?.Variable is SfaRealmVariable variable &&
                    variable.RenamedSystems?.GetValueOrDefault(id) is not null)
                {
                    var authorId = author.ProfileId.ToString("N");
                    variable.ReportSystem(id, authorId, author.Name);
                    r.SaveVariable();
                }
            });
        }

        public void ReportPlanetName(int id, SfaServerClient author)
        {
            if (author is null)
                return;

            RealmInfo?.Use(r =>
            {
                if (r.Realm?.Variable is SfaRealmVariable variable &&
                    variable.RenamedPlanets?.GetValueOrDefault(id) is not null)
                {
                    var authorId = author.ProfileId.ToString("N");
                    variable.ReportPlanet(id, authorId, author.Name);
                    r.SaveVariable();
                }
            });
        }

        public void SyncVariableMap(
            IEnumerable<RealmObjectRenameInfo> renamedSystems = null,
            IEnumerable<RealmObjectRenameInfo> renamedPlanets = null)
        {
            var doc = new JsonObject();

            if (renamedSystems is not null)
            {
                doc["renamed_systems"] = renamedSystems
                    .Where(i => i is not null)
                    .Select(i => JsonHelpers.ParseNodeUnbuffered(i))
                    .ToJsonArray();
            }

            if (renamedPlanets is not null)
            {
                doc["renamed_planets"] = renamedPlanets
                    .Where(i => i is not null)
                    .Select(i => JsonHelpers.ParseNodeUnbuffered(i))
                    .ToJsonArray();
            }

            if (doc.Count < 1)
                return;

            UseClients(clients =>
            {
                foreach (var client in clients)
                {
                    if (client.IsConnected == false)
                        continue;

                    client.Send(doc, SfaServerAction.SyncVariableMap);
                }
            });
        }
    }
}
