using StarfallAfterlife.Bridge.Game;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Launcher
{
    public class SfaSession
    {
        public SfaProfile Profile { get; set; }

        public SfaGame Game { get; protected set; }

        public Task<SfaSession> StartGame(string gameLocation, Uri serverAddress)
        {
            Game = new SfaGame()
            {
                Location = gameLocation,
                ServerAddress = serverAddress,
                Profile = Profile,
            };

            return Task<SfaSession>.Factory.StartNew(() =>
            {
                Game.Start();
                Game.Task?.Wait();
                return this;
            }, TaskCreationOptions.LongRunning);
        }
    }
}
