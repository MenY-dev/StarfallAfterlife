using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServerClient
    {
        public virtual void SyncAllQuests()
        {
            List<int> ids = new();

            try
            {
                foreach (var item in File.ReadAllLines(Path.Combine(".","Database", "quests.txt")))
                    if (int.TryParse(item, out int id))
                        ids.Add(id);

            }
            catch { }

            SendGalaxyMessage(
                DiscoveryServerGalaxyAction.QuestDataUpdate, writer => 
                {
                    JsonArray quests = new();
                    JsonObject doc = new();

                    for (int i = 0; i < ids.Count; i++)
                    {
                        int quest = ids[i];

                        quests.Add(new JsonObject
                        {
                            ["id"] = i,
                            ["entity"] = 0,
                            ["level"] = 0,
                            ["state"] = 0,
                            ["quest_logic"] = quest,
                        });
                    }

                    doc["quests"] = quests;
                    string outText = doc.ToJsonString();

                    SfaDebug.Print(outText.Length);

                    writer.WriteShortString(outText, -1, true, Encoding.UTF8);
                });
        }

        public virtual void SyncTestQuest(int logicId)
        {
            SendGalaxyMessage(
                DiscoveryServerGalaxyAction.QuestCompleteData,
                writer =>
                {
                    writer.WriteInt32(456789);
                    writer.WriteInt32(3);
                });

            SendGalaxyMessage(
                DiscoveryServerGalaxyAction.QuestDataUpdate, writer =>
                {
                    JsonObject doc = new()
                    {
                        ["quests"] = new JsonArray()
                        {
                            new JsonObject
                            {
                                ["id"] = 456789,
                                ["entity"] = 0,
                                ["level"] = 0,
                                ["state"] = 0,
                                ["quest_logic"] = logicId,
                            }
                        }
                    };

                    writer.WriteShortString(doc.ToJsonString(), -1, true, Encoding.UTF8);
                });
        }

        public virtual void SyncQuests()
        {
            SendGalaxyMessage(
                DiscoveryServerGalaxyAction.QuestDataUpdate,
                writer =>
                {
                    var doc = new JsonObject()
                    {
                        ["quests"] = new JsonArray()
                        {
                            new JsonObject
                            {
                                ["id"] = 0,
                                ["entity"] = 1300860703,
                                ["level"] = 27,
                                ["faction"] = 2,
                                ["state"] = 0,
                                ["quest_logic"] = 1569195116,
                                ["minutes_remain"] = 2,
                                ["progress"] = new JsonObject
                                {
                                    ["conditions"] = new JsonArray
                                    {
                                        new JsonObject
                                        {
                                            ["identity"] = "deliver_ore",
                                            ["progress_done"] = 11,
                                        }
                                    },
                                },
                                ["quest_params"] = new JsonObject
                                {
                                    ["condition_params"] = new JsonArray
                                    {
                                        new JsonObject
                                        {
                                            ["identity"] = "deliver_ore",
                                            ["item_to_deliver"] = 1300860703,
                                            ["dest_obj_id"] = 1300860703,
                                            ["dest_sys_id"] = 3,
                                            ["dest_obj_type"] = 0,
                                         }
                                    }
                                },
                                //["quest_params"] = new JsonObject
                                //{
                                //    ["conditions"] = new JsonArray
                                //    {
                                //        new JsonObject
                                //        {
                                //            ["identity"] = "deliver_primitives_1",
                                //            ["item_to_deliver"] = 1300860703
                                //        }
                                //    },
                                //},
                                //["quest_dialog"] = new JsonObject
                                //{

                                //}

                            }
                        }
                    };

                    writer.WriteShortString(doc.ToJsonString(), -1, true, Encoding.UTF8);
                });
        }

        public virtual void SendDiscoveryMessage(int systemId, DiscoveryObjectType objectType, int objectId, DiscoveryServerAction action, Action<SfWriter> writeAction = null)
        {
            SendToDiscoveryChannel(writer =>
            {
                writer.WriteInt32(systemId);
                writer.WriteByte((byte)objectType);
                writer.WriteInt32(objectId);
                writer.WriteByte((byte)action);
                writeAction?.Invoke(writer);
            });
        }

        public virtual void SendGalaxyMessage(DiscoveryServerGalaxyAction action, Action<SfWriter> writeAction = null)
        {
            if (writeAction is null)
                return;

            SendToGalacticChannel(writer =>
            {
                writer.WriteByte((byte)action);
                writeAction?.Invoke(writer);
            });
        }

        public virtual void SendToDiscoveryChannel(Action<SfWriter> writeAction)
        {
            if (writeAction is null)
                return;

            using SfWriter writer = new();
            writeAction?.Invoke(writer);
            Send(writer.ToArray(), SfaServerAction.DiscoveryChannel);
        }

        public virtual void SendToGalacticChannel(Action<SfWriter> writeAction)
        {
            if (writeAction is null)
                return;

            using SfWriter writer = new();
            writeAction?.Invoke(writer);
            Send(writer.ToArray(), SfaServerAction.GalacticChannel);
        }
    }
}
