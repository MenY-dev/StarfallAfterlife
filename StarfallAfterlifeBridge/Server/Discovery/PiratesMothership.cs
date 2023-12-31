﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class PiratesMothership : StarSystemDungeon
    {
        public float RespawnTimeout { get; set; } = 300;

        public DateTime RespawnTime { get; protected set; }

        public override void SetDungeonVisible(bool isVisible)
        {
            base.SetDungeonVisible(isVisible);

            if (isVisible == IsDungeonVisible)
                return;

            if (isVisible == false)
            {
                System?.AddDeferredAction(() =>
                {
                    if (System is null || IsDungeonVisible == true)
                        return;

                    SetDungeonVisible(true);
                }, TimeSpan.FromSeconds(RespawnTimeout));
            }

            System?.UpdateNavigationMap();
        }
    }
}
