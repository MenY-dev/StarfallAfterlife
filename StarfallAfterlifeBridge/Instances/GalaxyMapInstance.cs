using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Environment;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Networking.Channels;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class GalaxyMapInstance
    {
        public string Executable { get; set; }

        public string OutputDirectory { get; set; }

        public Process Process { get; set; }

        protected MgrServer GalaxyMgrServer { get; set; }

        protected GameChannelManager GalaxyMgrChannelManager { get; set; }

        public void Start()
        {
            Init();

            GalaxyMgrChannelManager.Start(new Uri("tcp://127.0.0.1:0"));
            SfaDebug.Print($"GalaxyMgrChannelManager Started! ({GalaxyMgrChannelManager.Address})");

            GalaxyMgrServer.Start(new Uri("http://127.0.0.1:0/galaxyinstancemgr/"));
            SfaDebug.Print($"GalaxyMgrServer Started! ({GalaxyMgrServer.Address})");

            WriteInstanceJson();

            Process = new SfaProcess
            {
                Executable = Executable,
                OutputDirectory = OutputDirectory,
                Listen = true,
                Map = "/Game/Maps/DistressBeacon",
                EnableLog = false,
                Windowed = true,
                DisableSplashScreen = true,
                DisableLoadingScreen = true,
            }.Start().InnerProcess;
        }

        protected void Init()
        {
            GalaxyMgrServer ??= new MgrServer(GalaxyInput);
            GalaxyMgrChannelManager ??= new GameChannelManager();
        }

        protected virtual void GalaxyInput(HttpListenerContext context, SfaHttpQuery query)
        {
            SfaDebug.Print(query.ToString(), "GalaxyMapInstance");
        }

        protected void WriteInstanceJson()
        {
            string instanceFile = Path.Combine(OutputDirectory, "instance.json");

            if (Directory.Exists(OutputDirectory) == false)
                Directory.CreateDirectory(OutputDirectory);

            File.WriteAllText(instanceFile, ToJsonString());
        }

        public string ToJsonString()
        {
            JsonNode doc = new JsonObject
            {
                ["sfmgr"] = new JsonObject
                {
                    ["url"] = GalaxyMgrServer?.Address.ToString(),
                    ["auth"] = "3f211507d042476ebc0d29db24881383"
                },
                ["realmmgr"] = new JsonObject
                {
                    ["url"] = GalaxyMgrServer?.Address.ToString(),
                    ["auth"] = "3f211507d042476ebc0d29db24881383"
                },
                ["galaxymgr"] = new JsonObject
                {
                    ["address"] = GalaxyMgrChannelManager?.Address.Host,
                    ["port"] = GalaxyMgrChannelManager?.Address.Port,
                    ["instanceid"] = 0,
                    ["systemid"] = 0,
                    ["auth"] = "3f211507d042476ebc0d29db24881383"
                },
                ["game_mode"] = "",
                ["is_custom_game"] = 0,
                ["mothership_income_override"] = 80,
                ["extradata"] = "",
                ["players_list"] = new JsonArray(),
                ["characters_list"] = new JsonArray(),
            };

            return doc.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
    }
}
