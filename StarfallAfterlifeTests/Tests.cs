using StarfallAfterlife.Bridge;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Game;
using StarfallAfterlife.Bridge.Generators;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Networking.Channels;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization.Json;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarfallAfterlife
{
    public class Tests
    {
        public string GameDirectory => Settings.Current.GameDirectory;

        public string AppLocation => Application.StartupPath;

        public string ExeLocation => Path.Combine(GameDirectory, "Msk", "starfall_game", "Starfall", "Binaries", "Win64", "Starfall.exe");

        public string LogsLocation => Path.Combine(GameDirectory, "Msk", "starfall_game", "Starfall.exe", "Starfall", "Saved", "Logs");

        public SfaGameProfile Profile { get; set; }

        public SfaGame Game { get; set; }

        public SfaServer Server { get; set; }

        public SfaLauncher Launcher { get; set; }

        public void StartGame()
        {

            string workingDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "My Games", "StarfallAfterlife");

            if (Launcher is null)
            {
                Launcher ??= new SfaLauncher()
                {
                    GameDirectory = GameDirectory,
                    WorkingDirectory = Path.Combine(workingDirectory),
                    Database = SfaDatabase.Instance,
                };

                Launcher.Load();
            }

            var serverAddress = StartServer();
            Launcher.StartGame(Launcher.Profiles.FirstOrDefault(), serverAddress);
        }

        public Uri StartServer()
        {
            if (Server is not null && Server.IsStarted == true)
                return Server.Address;

            var database = SfaDatabase.Instance;
            string workingDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "My Games", "StarfallAfterlife");

            var instanceManager = new InstanceManager()
            {
                GameExeLocation = ExeLocation,
                WorkingDirectory = Path.Combine(workingDirectory, "Instances")
            };

            instanceManager.Start();
            SfaDebug.Print($"InstanceManager Started! ({instanceManager.Address})");

            var realmId = "sfa_test_realm";
            var realmInfo = Launcher.Realms.FirstOrDefault(r => r.Realm.Id == realmId);

            if (realmInfo is null)
            {
                realmInfo = Launcher.CreateNewRealm(realmId);
                realmInfo.Realm.GalaxyMap = new TestGalaxyMapBuilder().Create(123, 10000);
                realmInfo.Realm.GalaxyMapHash = realmInfo.Realm.GalaxyMap.Hash;
                realmInfo.Realm.Database = database;
                realmInfo.Realm.MobsDatabase = new();
                realmInfo.Realm.QuestsDatabase = new QuestsGenerator(realmInfo.Realm).Build();
                realmInfo.Realm.ShopsMap = new ShopsGenerator(realmInfo.Realm).Build();
                realmInfo.Save();
            }
            else
            {
                realmInfo.LoadDatabase();
            }

            Server = realmInfo.Realm.CreateServer();
            Server.InstanceManagerAddress = instanceManager.Address;
            Server.Start();
            SfaDebug.Print($"DiscoveryServer Started! ({Server.Address})");

            return Server.Address;
        }

        public Uri StartServerOld()
        {
            if (Server is not null && Server.IsStarted == true)
                return Server.Address;

            string workingDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "My Games", "StarfallAfterlife");

            var database = SfaDatabase.Instance;
            var instanceManager = new InstanceManager()
            {
                GameExeLocation = ExeLocation,
                WorkingDirectory = Path.Combine(workingDirectory, "Instances")
            };

            instanceManager.Start();
            SfaDebug.Print($"InstanceManager Started! ({instanceManager.Address})");

            var realm = new SfaRealm();
            realm.GalaxyMap = new TestGalaxyMapBuilder().Create(123, 10000);
            realm.Database = database;
            realm.MobsDatabase = new();
            realm.QuestsDatabase = new QuestsGenerator(realm).Build();

            Server = realm.CreateServer();
            Server.InstanceManagerAddress = instanceManager.Address;
            Server.Start();
            SfaDebug.Print($"DiscoveryServer Started! ({Server.Address})");

            return Server.Address;
        }

        public void StartGameOld()
        {
            var database = SfaDatabase.Instance;
            var instanceManager = new InstanceManager()
            {
                GameExeLocation = ExeLocation,
                WorkingDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "SfaTests", "InstanceManager")
            };

            instanceManager.Start();
            SfaDebug.Print($"InstanceManager Started! ({instanceManager.Address})");

            var realm = new SfaRealm();
            realm.GalaxyMap = new TestGalaxyMapBuilder().Create(123, 10000);
            realm.Database = database;
            realm.MobsDatabase = new();
            realm.QuestsDatabase = new QuestsGenerator(realm).Build();

            var server = realm.CreateServer();
            server.InstanceManagerAddress = instanceManager.Address;
            server.Start();
            SfaDebug.Print($"DiscoveryServer Started! ({server.Address})");

            //Game = new SfaGame
            //{
            //    Realm = new SfaRealm
            //    {
            //        Database = database,
            //    },
            //    Location = GameDirectory,
            //    GameProfile = Profile,
            //    ServerAddress = server.Address,
            //};

            //Game?.Start();
        }

    }
}
