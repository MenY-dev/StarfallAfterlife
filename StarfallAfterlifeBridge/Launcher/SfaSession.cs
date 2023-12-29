﻿using StarfallAfterlife.Bridge.Game;
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

        public bool IsSuccess { get; protected set; }

        public string Reason { get; protected set; }

        public Func<string> PasswordRequested { get; set; }


        public Task<SfaSession> StartGame(string gameLocation, Uri serverAddress, IProgress<string> progress = null)
        {
            Game = new SfaGame()
            {
                Location = gameLocation,
                ServerAddress = serverAddress,
                Profile = Profile,
                PasswordRequested = PasswordRequested,
            };

            Game.Start(progress);

            return Game.StartingTask.ContinueWith(t =>
            {
                var result = t.Result;
                IsSuccess = result.IsSucces;
                Reason = result.Reason;
                progress?.Report("stopped");
                return this;
            });
        }
    }
}
